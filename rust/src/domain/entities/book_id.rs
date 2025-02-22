use std::str::FromStr;

use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Serialize, Deserialize)]
pub struct BookId {
    id: Uuid,
}

impl BookId {
    pub fn new(id: String) -> BookId {
        let uuid = Uuid::from_str(&id).unwrap();
        BookId { id: uuid }
    }
}
