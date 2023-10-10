using PublishingHouse.Books;
using PublishingHouse.Books.Authors;
using PublishingHouse.Books.Draft;
using PublishingHouse.Books.Entities;
using PublishingHouse.Books.Factories;
using PublishingHouse.Books.Initial;
using PublishingHouse.Books.InPrint;
using PublishingHouse.Books.Published;
using PublishingHouse.Books.Publishers;
using PublishingHouse.Books.Services;
using PublishingHouse.Books.UnderEditing;
using PublishingHouse.Core.ValueObjects;
using PublishingHouse.Persistence.Books.Mappers;
using PublishingHouse.Persistence.Books.Repositories;

namespace PublishingHouse.Application.Books;

using static BookCommand;

public class BooksService: IBooksService
{
    public async Task CreateDraft(CreateDraft command, CancellationToken ct)
    {
        var (bookId, title, author, publisherId, edition, genre) = command;
        var authorEntity = await authorProvider.GetOrCreate(author, ct);
        var publisherEntity = await publisherProvider.GetById(publisherId, ct);

        await Handle<InitialBook>(
            bookId,
            book =>
                book.CreateDraft(
                    title,
                    authorEntity,
                    publisherEntity,
                    edition,
                    genre
                ), ct);
    }

    public Task AddChapter(AddChapter command, CancellationToken ct) =>
        Handle<BookDraft>(command.BookId, book =>
        {
            var (_, chapterTitle, chapterContent) = command;
            return book.AddChapter(chapterTitle, chapterContent);
        }, ct);

    public Task MoveToEditing(MoveToEditing command, CancellationToken ct) =>
        Handle<BookDraft>(command.BookId, book => book.MoveToEditing(), ct);

    public Task AddTranslation(AddTranslation command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId,
            book => book.AddTranslation(command.Translation, maximumNumberOfTranslations), ct);

    public Task AddFormat(AddFormat command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId, book => book.AddFormat(command.Format), ct);

    public Task RemoveFormat(RemoveFormat command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId, book => book.RemoveFormat(command.Format), ct);

    public Task AddReviewer(AddReviewer command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId, book => book.AddReviewer(command.Reviewer), ct);

    public Task Approve(Approve command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId,
            book => book.Approve(command.CommitteeApproval, minimumReviewersRequiredForApproval), ct);

    public Task SetISBN(SetISBN command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId, book => book.SetISBN(command.ISBN), ct);

    public Task MoveToPrinting(MoveToPrinting command, CancellationToken ct) =>
        Handle<BookUnderEditing>(command.BookId, book => book.MoveToPrinting((null as IPublishingHouse)!), ct);

    public Task MoveToPublished(MoveToPublished command, CancellationToken ct) =>
        Handle<BookInPrint>(command.BookId, book => book.MoveToPublished(), ct);

    public Task MoveToOutOfPrint(MoveToOutOfPrint command, CancellationToken ct) =>
        Handle<PublishedBook>(command.BookId,
            book => book.MoveToOutOfPrint(maxAllowedUnsoldCopiesRatioToGoOutOfPrint), ct);

    private Task Handle<T>(BookId id, Func<T, BookEvent> handle, CancellationToken ct) where T : Book =>
        repository.GetAndUpdate(id, (entity) =>
        {
            var aggregate = entity?.MapToAggregate(bookFactory) ?? GetDefault();

            if (aggregate is not T typedBook) throw new InvalidOperationException();

            var @event = handle(typedBook);

            return new[] { @event };
        }, ct);

    private Book GetDefault() => new InitialBook();

    public BooksService(
        IBooksRepository repository,
        IBookFactory bookFactory,
        IAuthorProvider authorProvider,
        IPublisherProvider publisherProvider,
        PositiveInt minimumReviewersRequiredForApproval,
        PositiveInt maximumNumberOfTranslations,
        Ratio maxAllowedUnsoldCopiesRatioToGoOutOfPrint
    )
    {
        this.repository = repository;
        this.bookFactory = bookFactory;
        this.authorProvider = authorProvider;
        this.publisherProvider = publisherProvider;
        this.minimumReviewersRequiredForApproval = minimumReviewersRequiredForApproval;
        this.maximumNumberOfTranslations = maximumNumberOfTranslations;
        this.maxAllowedUnsoldCopiesRatioToGoOutOfPrint = maxAllowedUnsoldCopiesRatioToGoOutOfPrint;
    }

    private readonly IBooksRepository repository;
    private readonly IBookFactory bookFactory;
    private readonly IAuthorProvider authorProvider;
    private readonly IPublisherProvider publisherProvider;
    private readonly PositiveInt minimumReviewersRequiredForApproval;
    private readonly PositiveInt maximumNumberOfTranslations;
    private readonly Ratio maxAllowedUnsoldCopiesRatioToGoOutOfPrint;
}
