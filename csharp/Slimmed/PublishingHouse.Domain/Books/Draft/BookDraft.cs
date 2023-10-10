using PublishingHouse.Books.Entities;

namespace PublishingHouse.Books.Draft;
using static PublishingHouse.Books.BookEvent;
using static PublishingHouse.Books.BookEvent.DraftEvent;
using static PublishingHouse.Books.BookEvent.UnderEditingEvent;
using static PublishingHouse.Books.BookEvent.InPrintEvent;
using static PublishingHouse.Books.BookEvent.PublishedEvent;
using static PublishingHouse.Books.BookEvent.OutOfPrintEvent;


public class BookDraft: Book
    {
        private readonly Genre? genre;
        private readonly List<ChapterTitle> chapterTitles;
        private int ChaptersCount => chapterTitles.Count;

        internal BookDraft(
            BookId bookId,
            Genre? genre,
            List<ChapterTitle> chapterTitles
        ): base(bookId)
        {
            this.genre = genre;
            this.chapterTitles = chapterTitles;
        }

        public ChapterAdded AddChapter(ChapterTitle title, ChapterContent content)
        {
            if (!title.Value.StartsWith("Chapter " + (chapterTitles.Count + 1)))
                throw new InvalidOperationException(
                    $"Chapter should be added in sequence. The title of the next chapter should be 'Chapter {chapterTitles.Count + 1}'");

            if (chapterTitles.Contains(title))
                throw new InvalidOperationException($"Chapter with title {title.Value} already exists.");

            var chapter = new Chapter(new ChapterNumber(chapterTitles.Count + 1), title, content);
            chapterTitles.Add(title);

            return new ChapterAdded(Id, chapter);
        }

        public MovedToEditing MoveToEditing()
        {
            if (ChaptersCount < 1)
                throw new InvalidOperationException(
                    "A book must have at least one chapter to move to the Editing state.");

            if (genre == null)
                throw new InvalidOperationException("Book can be moved to the editing only when genre is specified");

            return new MovedToEditing(Id, genre);
        }

        public static BookDraft Evolve(BookDraft book, DraftEvent @event) =>
            @event switch
            {
                DraftCreated draftCreated =>
                    new BookDraft(
                        draftCreated.BookId,
                        draftCreated.Genre,
                        new List<ChapterTitle>()
                    ),

                ChapterAdded chapterAdded =>
                    new BookDraft(
                        chapterAdded.BookId,
                        book.genre,
                        book.chapterTitles.Union(new[] { chapterAdded.Chapter.Title }).ToList()
                    ),
                _ => book
            };
    }
