package main

import (
	"fmt"
	"reflect"
	"strings"
)

func richMetaType(T any) func() string {
	return func() string {
		return strings.Split(reflect.TypeOf(T).String(), ".")[1]
	}
}

type state struct {
	getEmbeddingType func() string
}

func (s *state) __state() string {
	// Use the embedded function to get the embedding struct's type
	return s.getEmbeddingType()
}

type BookState interface {
	__state() string
}

type Initial struct {
	state
}

func (Initial) New() BookState {
	i := &Initial{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type Draft struct {
	state
}

func (Draft) New() BookState {
	i := &Draft{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type UnderEditing struct {
	state
}

func (UnderEditing) New() BookState {
	i := &UnderEditing{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type PublishedBook struct {
	state
}

func (PublishedBook) New() BookState {
	i := &PublishedBook{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type InPrint struct {
	state
}

func (InPrint) New() BookState {
	i := &InPrint{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type OutOfPrint struct {
	state
}

func (OutOfPrint) New() BookState {
	i := &OutOfPrint{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

// export type Book =
//   | Initial
//   | Draft
//   | UnderEditing
//   | PublishedBook
//   | InPrint
//   | OutOfPrint;

type event struct {
	getEmbeddingType func() string
}

func (e *event) __event() string {
	return e.getEmbeddingType()
}

type BookEvent interface {
	__event() string
}

type DraftEvent struct {
	event
}

func (DraftEvent) New() BookEvent {
	i := &DraftEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type UnderEditingEvent struct {
	event
}

func (UnderEditingEvent) New() BookEvent {
	i := &UnderEditingEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type InPrintEvent struct {
	event
}

func (InPrintEvent) New() BookEvent {
	i := &InPrintEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type PublishedEvent struct {
	event
}

func (PublishedEvent) New() BookEvent {
	i := &PublishedEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type OutOfPrintEvent struct {
	event
}

func (OutOfPrintEvent) New() BookEvent {
	i := &OutOfPrintEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

// export type BookEvent =
//   | DraftEvent
//   | UnderEditingEvent
//   | InPrintEvent
//   | PublishedEvent
//   | OutOfPrintEvent;

func main() {
	states := []BookState{
		Initial{}.New(),
		Draft{}.New(),
		UnderEditing{}.New(),
		PublishedBook{}.New(),
		InPrint{}.New(),
		OutOfPrint{}.New(),
	}

	for _, state := range states {
		fmt.Printf("State Type: %s\n", state.__state())
	}

	events := []BookEvent{DraftEvent{}.New(), UnderEditingEvent{}.New(), InPrintEvent{}.New(), PublishedEvent{}.New(), OutOfPrintEvent{}.New()}

	for _, event := range events {
		fmt.Printf("Event Type %s\n", event.__event())
	}

}
