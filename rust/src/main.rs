use actix_web::{App, HttpServer};
use api::controller::book::books;

#[actix_web::main]
async fn main() -> std::io::Result<()> {
    HttpServer::new(|| App::new().service(books))
        .bind(("127.0.0.1", 8080))?
        .run()
        .await
}
