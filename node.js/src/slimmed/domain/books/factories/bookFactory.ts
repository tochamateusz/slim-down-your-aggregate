import { State, Book } from '../book';
import {
  Author,
  Chapter,
  CommitteeApproval,
  Format,
  Genre,
  ISBN,
  Reviewer,
  Title,
  Translation,
} from '../entities';

export interface IBookFactory {
  create(
    state: State,
    title: Title,
    author: Author,
    genre: Genre | null,
    isbn: ISBN | null,
    committeeApproval: CommitteeApproval | null,
    reviewers: Reviewer[] | null,
    chapters: Chapter[] | null,
    translations: Translation[] | null,
    formats: Format[] | null,
  ): Book;
}
