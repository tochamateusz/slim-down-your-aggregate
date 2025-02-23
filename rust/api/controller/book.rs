use actix_web::{post, HttpResponse, Responder};

use crate::requests::create_draft::to_create_draft_cmd;


#[post("/api/books")]
pub async fn books(req_body: String) -> impl Responder {
    let cmd=to_create_draft_cmd(req_body.to_string());

    match cmd {
        Ok(cmd)=>{
            println!("/api/books/{:?} | {:?}", cmd.book_id, cmd);
            HttpResponse::Ok().body(req_body)
        }
        Err(e)=>HttpResponse::BadRequest().body(e.to_string())
    }
}
