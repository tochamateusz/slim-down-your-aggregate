use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
pub struct Title {
    title: String,
}

impl Title {
    pub fn new(titile: String) -> Self {
        Title { title: titile }
    }
}
