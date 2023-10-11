import { IBooksRepository } from '../../persistence/books/repositories';
import { IAuthorProvider } from '../../domain/books/authors';
import { IPublisherProvider } from '../../domain/books/publishers/publisherProvider';
import { Book } from '../../domain/books/book';
import { InvalidOperationError } from '#core/errors';
import {
  AddChapter,
  AddFormat,
  AddTranslation,
  Approve,
  CreateDraft,
  MoveToEditing,
  MoveToOutOfPrint,
  MoveToPrinting,
  MoveToPublished,
  RemoveFormat,
  AddReviewer,
  SetISBN,
} from './bookCommand';
import { PositiveNumber } from '#core/typing';
import { Ratio } from '#core/typing/ratio';
import { BookId } from '../../domain/books/entities';
import { BookEvent } from '../../domain/books/book';
import { IBookFactory } from '../../domain/books/factories';
import { bookMapper } from '../../persistence/mappers/bookMapper';
import { Draft, Initial } from '../../domain/books/draft';
import { InPrint } from '../../domain/books/inPrint';
import { PublishedBook } from '../../domain/books/published';
import { UnderEditing } from '../../domain/books/underEditing';

export interface IBooksService {
  createDraft(command: CreateDraft): Promise<void>;
  addChapter(command: AddChapter): Promise<void>;
  moveToEditing(command: MoveToEditing): Promise<void>;
  addTranslation(command: AddTranslation): Promise<void>;
  addFormat(command: AddFormat): Promise<void>;
  removeFormat(command: RemoveFormat): Promise<void>;
  addReviewer(command: AddReviewer): Promise<void>;
  approve(command: Approve): Promise<void>;
  setISBN(command: SetISBN): Promise<void>;
  moveToPrinting(command: MoveToPrinting): Promise<void>;
  moveToPublished(command: MoveToPublished): Promise<void>;
  moveToOutOfPrint(command: MoveToOutOfPrint): Promise<void>;
}

export class BooksService implements IBooksService {
  public createDraft = async (command: CreateDraft): Promise<void> => {
    const { bookId, title, author, publisherId, edition, genre } = command.data;

    const authorEntity = await this.authorProvider.getOrCreate(author);
    const publisherEntity = await this.publisherProvider.getById(publisherId);

    return this.handle(bookId, (book) => {
      if (!(book instanceof Initial))
        throw InvalidOperationError('Invalid State');

      return Draft.createDraft(
        book,
        title,
        authorEntity,
        publisherEntity,
        edition,
        genre,
      );
    });
  };

  public addChapter = async (command: AddChapter): Promise<void> => {
    return this.handle(command.data.bookId, (book) => {
      if (!(book instanceof Draft))
        throw InvalidOperationError('Invalid State');

      const { chapterTitle, chapterContent } = command.data;

      return Draft.addChapter(book, chapterTitle, chapterContent);
    });
  };

  public moveToEditing = async (command: MoveToEditing): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof Draft))
        throw InvalidOperationError('Invalid State');

      return Draft.moveToEditing(book);
    });

  public addTranslation = async (command: AddTranslation): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { translation } = command.data;

      return UnderEditing.addTranslation(
        book,
        translation,
        this.maximumNumberOfTranslations,
      );
    });

  public addFormat = async (command: AddFormat): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { format } = command.data;

      return UnderEditing.addFormat(book, format);
    });

  public removeFormat = async (command: RemoveFormat): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { format } = command.data;

      return UnderEditing.removeFormat(book, format);
    });

  public addReviewer = async (command: AddReviewer): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { reviewer } = command.data;

      return UnderEditing.addReviewer(book, reviewer);
    });

  public approve = async (command: Approve): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { committeeApproval } = command.data;

      return UnderEditing.approve(
        book,
        committeeApproval,
        this.minimumReviewersRequiredForApproval,
      );
    });

  public setISBN = async (command: SetISBN): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      const { isbn } = command.data;

      return UnderEditing.setISBN(book, isbn);
    });

  public moveToPrinting = async (command: MoveToPrinting): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof UnderEditing))
        throw InvalidOperationError('Invalid State');

      return UnderEditing.moveToPrinting(book, {
        isGenreLimitReached: () => true,
      });
    });

  public moveToPublished = async (command: MoveToPublished): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof InPrint))
        throw InvalidOperationError('Invalid State');

      return InPrint.moveToPublished(book);
    });

  public moveToOutOfPrint = async (command: MoveToOutOfPrint): Promise<void> =>
    this.handle(command.data.bookId, (book) => {
      if (!(book instanceof PublishedBook))
        throw InvalidOperationError('Invalid State');

      return PublishedBook.moveToOutOfPrint(
        book,
        this.maxAllowedUnsoldCopiesRatioToGoOutOfPrint,
      );
    });

  private handle = (
    id: BookId,
    handle: (book: Book) => BookEvent | BookEvent[],
  ): Promise<void> => {
    return this.repository.getAndUpdate(id, (entity) => {
      const aggregate =
        entity !== null
          ? bookMapper.mapFromEntity(entity, this.bookFactory)
          : this.getDefault();

      const result = handle(aggregate);
      return Array.isArray(result) ? result : [result];
    });
  };

  private getDefault = (): Book => Initial.initial;

  constructor(
    private readonly repository: IBooksRepository,
    private readonly bookFactory: IBookFactory,
    private readonly authorProvider: IAuthorProvider,
    private readonly publisherProvider: IPublisherProvider,
    private readonly minimumReviewersRequiredForApproval: PositiveNumber,
    private readonly maximumNumberOfTranslations: PositiveNumber,
    private readonly maxAllowedUnsoldCopiesRatioToGoOutOfPrint: Ratio,
  ) {}
}
