import {
  parseNonEmptyString,
  parseNonEmptyUuid,
  parsePositiveNumber,
} from '#core/typing';
import { Request } from 'express';
import { CreateDraftCommand } from 'src/original/application/books/commands/createDraftCommand';
import { BookId } from 'src/original/domain/books/entities';
import { DeepReadonly } from 'ts-essentials';

export type CreateDraftRequest = DeepReadonly<
  Request<
    unknown,
    unknown,
    Partial<{
      title: string;
      author:
        | { authorId: string }
        | {
            firstName: string;
            lastName: string;
          };
      publisherId: string;
      edition: number;
      genre: string | undefined;
    }>
  >
>;

export const toCreateDraftCommand = (
  bookId: BookId,
  request: CreateDraftRequest,
): CreateDraftCommand => {
  const { title, publisherId, author, edition, genre } = {
    author: { authorId: undefined },
    ...request.body,
  };

  return {
    type: 'CreateDraftCommand',
    data: {
      bookId,
      title: parseNonEmptyString(title),
      author:
        'authorId' in author
          ? parseNonEmptyString(author.authorId)
          : {
              firstName: parseNonEmptyString(author.firstName),
              lastName: parseNonEmptyString(author.lastName),
            },
      publisherId: parseNonEmptyUuid(publisherId),
      edition: parsePositiveNumber(edition),
      genre: genre !== undefined ? parseNonEmptyString(genre) : null,
    },
  };
};
