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

func (s state) __state() string {
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

	Genre    *string
	Chapters []Chapter
}

func (Draft) initialDraft() Draft {
	i := Draft{
		Genre:    nil,
		Chapters: []Chapter{},
	}
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

func (e event) __event() string {
	return e.getEmbeddingType()
}

type BookEvent interface {
	__event() string
}

type external_event struct {
}

func (external_event) __external_event(e event) string {
	return e.getEmbeddingType()
}

type DraftEvent interface {
	__draft_event(e event) string
}

type draft_event struct {
	external_event
}

func (draft_event) __draft_event(e event) string {
	return e.getEmbeddingType()
}

type BookExternalEvent interface {
	__external_event(e event) string
}

// export type BookExternalEvent =
//   | DraftEvent
//   | UnderEditingEvent
//   | PublishedExternal
//   | OutOfPrintEvent;

// export type DraftEvent = DraftCreated | ChapterAdded;

type DraftCreated struct {
	event
	draft_event

	Genre string
}

func (DraftCreated) New() *DraftCreated {
	i := &DraftCreated{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type Chapter struct {
	Number  int64
	Title   string
	Content string
}

type ChapterAdded struct {
	event
	draft_event

	Chapter Chapter
}

func (ChapterAdded) New() *ChapterAdded {
	i := &ChapterAdded{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

func evolveDraft(state Draft, event DraftEvent) Draft {
	switch e := event.(type) {
	default:
		{
			return state
		}
	case DraftCreated:
		{
			state.Genre = &e.Genre
			return state
		}
	case ChapterAdded:
		{
			state.Chapters = append(state.Chapters, e.Chapter)
			return state
		}
	}
}

type UnderEditingEvent struct {
	event
	external_event
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
	external_event
}

func (PublishedEvent) New() BookEvent {
	i := &PublishedEvent{}
	i.getEmbeddingType = richMetaType(i)
	return i
}

type OutOfPrintEvent struct {
	event
	external_event
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

func isInitial(state BookState) bool {
	_, ok := state.(Initial)
	return ok
}

func isDraft(state BookState) bool {
	_, ok := state.(Draft)
	return ok
}

func evolve(state BookState, event BookEvent) BookState {
	switch e := event.(type) {
	case DraftCreated:
		{
			if !isInitial(state) || !isDraft(state) {
				return state
			}
			return evolveDraft(Draft{}.initialDraft(), e)
		}
	case ChapterAdded:
		{
			if !isDraft(state) {
				return state
			}
			return evolveDraft(state.(Draft), e)
		}
	case UnderEditingEvent:
		{
			return state
		}
	case InPrintEvent:
		{

			return state
		}
	case PublishedEvent:
		{
			return state
		}
	case OutOfPrintEvent:
		{
			return state
		}
	default:
		{
			return state
		}
	}
}

func main() {
	states := []BookState{
		Initial{}.New(),
		Draft{}.initialDraft(),
		UnderEditing{}.New(),
		PublishedBook{}.New(),
		InPrint{}.New(),
		OutOfPrint{}.New(),
	}

	for _, state := range states {
		fmt.Printf("State Type: %s\n", state.__state())
	}

	events := []BookEvent{DraftCreated{}.New(), ChapterAdded{}.New(), UnderEditingEvent{}.New(), InPrintEvent{}.New(), PublishedEvent{}.New(), OutOfPrintEvent{}.New()}

	for _, event := range events {
		fmt.Printf("Event Type %s\n", event.__event())
	}

}
