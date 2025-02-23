struct Initial {}
struct Draft {}
struct UnderEditing {}
struct PublishedBook {}
struct InPrint {}
struct OutOfPrint {}

enum BookStates {
    Initial(Initial),
    Draft(Draft),
    UnderEditing(UnderEditing),
    InPrint(InPrint),
    OutOfPrint(OutOfPrint),
}

struct Book {
    state: BookStates,
}
