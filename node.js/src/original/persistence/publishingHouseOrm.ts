import { AuthorEntity } from './authors';
import { BookEntity } from './books';
import { OutboxMessageEntity } from './core/outbox/outboxMessageEntity';
import { Database, EntitiesCollection, getDatabase } from './orm';
import { PublisherEntity } from './publishers';

export interface PublishingHouseOrm extends Database {
  authors: EntitiesCollection<AuthorEntity>;
  books: EntitiesCollection<BookEntity>;
  publishers: EntitiesCollection<PublisherEntity>;
  outbox: EntitiesCollection<OutboxMessageEntity>;
}

export const publishingHouseOrm = (): PublishingHouseOrm => {
  const database = getDatabase();

  return {
    ...database,
    authors: database.table<AuthorEntity>('authors'),
    books: database.table<BookEntity>('books'),
    publishers: database.table<PublisherEntity>('publishers'),
    outbox: database.table<OutboxMessageEntity>('outbox'),
  };
};
