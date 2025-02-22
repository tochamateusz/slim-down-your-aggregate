use crate::applications::book::book_services::CreateDraftAndSetupAuthorAndPublisher;
use crate::domain::entities::{book_id::BookId, title::Title};
use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
struct CreateDraftRequest {
    book_id: String,
    title: String,
}

#[derive(Debug)]
pub struct CreateDraftError;

impl std::fmt::Display for CreateDraftError {
    fn fmt(&self, f: &mut std::fmt::Formatter) -> std::fmt::Result {
        write!(f, "can't create draft command")
    }
}

pub fn to_create_draft_cmd(
    body: String,
) -> Result<CreateDraftAndSetupAuthorAndPublisher, serde_json::error::Error> {
    let serizalized: Result<CreateDraftRequest, _> = serde_json::from_str(&body);

    match serizalized {
        Ok(serizalized) => Ok(CreateDraftAndSetupAuthorAndPublisher {
            book_id: BookId::new(serizalized.book_id),
            title: Title::new(serizalized.title),
        }),
        Err(e) => Err(e),
    }
}

// export const toCreateDraftCommand = (
//   bookId: BookId,
//   request: CreateDraftRequest,
// ): CreateDraftAndSetupAuthorAndPublisher => {
//   const { title, publisherId, author, edition, genre } = {
//     author: { authorId: undefined },
//     ...request.body,
//   };

//   return {
//     type: 'CreateDraftCommand',
//     data: {
//       bookId,
//       title: parseNonEmptyString(title),
//       author:
//         'authorId' in author
//           ? parseNonEmptyString(author.authorId)
//           : {
//               firstName: parseNonEmptyString(author.firstName),
//               lastName: parseNonEmptyString(author.lastName),
//             },
//       publisherId: parseNonEmptyUuid(publisherId),
//       edition: parsePositiveNumber(edition),
//       genre: genre !== undefined ? parseNonEmptyString(genre) : null,
//     },
//   };
// };
