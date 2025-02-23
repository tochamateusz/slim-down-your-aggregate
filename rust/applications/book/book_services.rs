use domain::entities::{book_id::BookId, title::Title};

#[derive(Debug)]
pub struct CreateDraftAndSetupAuthorAndPublisher {
    pub book_id: BookId,
    pub title: Title,
    // author: AuthorIdOrData;
    // publisherId: PublisherId;
    // edition: PositiveNumber;
    // genre: Genre | null;
}

// export type CreateDraftAndSetupAuthorAndPublisher = Command<
//   'CreateDraftCommand',
//   {
//     bookId: BookId;
//     title: Title;
//     author: AuthorIdOrData;
//     publisherId: PublisherId;
//     edition: PositiveNumber;
//     genre: Genre | null;
//   }
// >;
//
