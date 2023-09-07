using PublishingHouse.Books.Entities;
using PublishingHouse.Books.Events;
using PublishingHouse.Books.Factories;
using PublishingHouse.Books.Services;
using PublishingHouse.Core;

namespace PublishingHouse.Books;

public class Book: Aggregate
{
    public enum State { Writing, Editing, Printing, Published, OutOfPrint }

    public BookId BookId { get; }
    public State CurrentState { get; private set; }
    public Title Title { get; }
    public Author Author { get; }
    public Genre? Genre { get; }
    public Publisher Publisher { get; }
    public int Edition { get; }
    public ISBN? ISBN { get; }
    public DateTime? PublicationDate { get; }
    public int? TotalPages { get; }
    public int? NumberOfIllustrations { get; }
    public string? BindingType { get; }

    //TODO: add type for that
    public string? Summary { get; }

    private readonly IPublishingHouse publishingHouse;

    public IReadOnlyList<Reviewer> Reviewers => reviewers.AsReadOnly();
    private readonly List<Reviewer> reviewers;
    public IReadOnlyList<Chapter> Chapters => chapters.AsReadOnly();
    private readonly List<Chapter> chapters;
    public CommitteeApproval? CommitteeApproval { get; private set; }
    public IReadOnlyList<Translation> Translations => translations.AsReadOnly();
    private readonly List<Translation> translations;
    public IReadOnlyList<Format> Formats => formats.AsReadOnly();
    private readonly List<Format> formats;

    public static Book CreateDraft(
        BookId bookId,
        Title title,
        Author author,
        Genre? genre,
        IPublishingHouse publishingHouse,
        Publisher publisher,
        int edition
    ) =>
        new Book(bookId, State.Writing, title, author, genre, publishingHouse, publisher, edition);

    public void AddChapter(ChapterTitle title, ChapterContent content)
    {
        if (chapters.Any(chap => chap.Title.Value == title.Value))
            throw new InvalidOperationException($"Chapter with title {title.Value} already exists.");

        if (chapters.Count > 0 && chapters.Last().Title.Value != "Chapter " + (chapters.Count))
            throw new InvalidOperationException(
                $"Chapter should be added in sequence. The title of the next chapter should be 'Chapter {chapters.Count + 1}'");

        var chapter = new Chapter(title, content);
        chapters.Add(chapter);

        AddDomainEvent(new ChapterAddedEvent(BookId, chapter));
    }

    public void MoveToEditing()
    {
        if (CurrentState != State.Writing)
            throw new InvalidOperationException("Cannot move to Editing state from the current state.");

        if (chapters.Count < 1)
            throw new InvalidOperationException("A book must have at least one chapter to move to the Editing state.");

        if (Genre == null)
            throw new InvalidOperationException("Book can be moved to the editing only when genre is specified");

        CurrentState = State.Editing;

        AddDomainEvent(new BookMovedToEditingEvent(BookId));
    }

    public void AddTranslation(Translation translation)
    {
        if (CurrentState != State.Editing)
            throw new InvalidOperationException("Cannot add translation of a book that is not in the Editing state.");

        if (translations.Count >= 5)
            throw new InvalidOperationException("Cannot add more translations. Maximum 5 translations are allowed.");

        translations.Add(translation);
    }

    public void AddFormat(Format format)
    {
        if (CurrentState != State.Editing)
            throw new InvalidOperationException("Cannot add format of a book that is not in the Editing state.");

        if (formats.Any(f => f.FormatType == format.FormatType))
            throw new InvalidOperationException($"Format {format.FormatType} already exists.");

        formats.Add(format);
    }

    public void RemoveFormat(Format format)
    {
        if (CurrentState != State.Editing)
            throw new InvalidOperationException("Cannot remove format of a book that is not in the Editing state.");

        var existingFormat = formats.FirstOrDefault(f => f.FormatType == format.FormatType);
        if (existingFormat == null)
            throw new InvalidOperationException($"Format {format.FormatType} does not exist.");

        formats.Remove(existingFormat);
    }

    public void Approve(CommitteeApproval committeeApproval)
    {
        if (CurrentState != State.Editing)
            throw new InvalidOperationException("Cannot approve a book that is not in the Editing state.");

        if (Reviewers.Count < 3)
            throw new InvalidOperationException(
                "A book cannot be approved unless it has been reviewed by at least three reviewers.");

        CommitteeApproval = committeeApproval;
    }

    public void MoveToPrinting()
    {
        if (CurrentState != State.Editing)
            throw new InvalidOperationException("Cannot move to printing from the current state.");

        if (CommitteeApproval == null)
            throw new InvalidOperationException("Cannot move to printing state until the book has been approved.");

        if (Reviewers.Count < 3)
            throw new InvalidOperationException(
                "A book cannot be moved to the Printing state unless it has been reviewed by at least three reviewers.");

        if (Genre == null)
            throw new InvalidOperationException("Book can be moved to the printing only when genre is specified");

        if (!publishingHouse.IsGenreLimitReached(Genre))
            throw new InvalidOperationException("Cannot move to printing until the genre limit is reached.");

        CurrentState = State.Printing;
    }

    public void MoveToPublished()
    {
        if (CurrentState != State.Printing || translations.Count < 5)
            throw new InvalidOperationException("Cannot move to Published state from the current state.");

        if (ISBN == null)
            throw new InvalidOperationException("Cannot move to Published state without ISBN.");

        if (Reviewers.Count < 3)
            throw new InvalidOperationException(
                "A book cannot be moved to the Published state unless it has been reviewed by at least three reviewers.");

        CurrentState = State.Published;

        AddDomainEvent(new BookPublishedEvent(BookId, ISBN, Title, Author));
    }

    public void MoveToOutOfPrint()
    {
        if (CurrentState != State.Published)
            throw new InvalidOperationException("Cannot move to Out of Print state from the current state.");

        double totalCopies = formats.Sum(f => f.TotalCopies);
        double totalSoldCopies = formats.Sum(f => f.SoldCopies);
        if ((totalSoldCopies / totalCopies) > 0.1)
            throw new InvalidOperationException(
                "Cannot move to Out of Print state if more than 10% of total copies are unsold.");

        CurrentState = State.OutOfPrint;
    }

    private Book(
        BookId bookId,
        State state,
        Title title,
        Author author,
        Genre? genre,
        IPublishingHouse publishingHouse,
        Publisher publisher,
        int edition,
        ISBN? isbn = null,
        DateTime? publicationDate = null,
        int? totalPages = null,
        int? numberOfIllustrations = null,
        string? bindingType = null,
        string? summary = null,
        CommitteeApproval? committeeApproval = null,
        List<Reviewer>? reviewers = null,
        List<Chapter>? chapters = null,
        List<Translation>? translations = null,
        List<Format>? formats = null
    ): base(bookId.Value)
    {
        BookId = bookId;
        CurrentState = state;
        Title = title;
        Author = author;
        Genre = genre;
        this.publishingHouse = publishingHouse;
        Publisher = publisher;
        Edition = edition;
        ISBN = isbn;
        PublicationDate = publicationDate;
        TotalPages = totalPages;
        NumberOfIllustrations = numberOfIllustrations;
        BindingType = bindingType;
        Summary = summary;
        CommitteeApproval = committeeApproval;
        this.reviewers = reviewers ?? new List<Reviewer>();
        this.chapters = chapters ?? new List<Chapter>();
        this.translations = translations?? new List<Translation>();
        this.formats = formats ?? new List<Format>();
    }

    public class Factory: IBooksFactory
    {
        public Book Create(
            BookId bookId,
            State state,
            Title title,
            Author author,
            Genre? genre,
            IPublishingHouse publishingHouse,
            Publisher publisher,
            int edition,
            ISBN? isbn,
            DateTime? publicationDate,
            int? totalPages,
            int? numberOfIllustrations,
            string? bindingType,
            string? summary,
            CommitteeApproval? committeeApproval,
            List<Reviewer> reviewers,
            List<Chapter> chapters,
            List<Translation> translations,
            List<Format> formats
        ) =>
            new Book(
                bookId, state, title, author, genre, publishingHouse, publisher,
                edition, isbn, publicationDate, totalPages, numberOfIllustrations,
                bindingType, summary, committeeApproval, reviewers, chapters, translations, formats);
    }
}
