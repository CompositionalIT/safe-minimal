module safetest.Client

open Elmish
open Elmish.React
open Fable.React
open Fable.React.Props
open safetest.Shared
open Thoth.Fetch

// The model holds state that you want to keep track of while the application is running
// In this case, we are keeping track of list of Todos and Input value
// The Input denotes value for a new todo to be added
type Model =
    { Todos: Todo list
      Input: string }

// The Msg type defines what events/actions can occur while the application is running
// The state of the application changes only in reaction to these events
type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

// Below functions send HTTP requests to server
let getTodos() = Fetch.get<unit, Todo list> Routes.todos
let addTodo(todo) = Fetch.post<Todo, Todo> (Routes.todos, todo)

// Init function defines initial state (model) and command (side effect) of the application
// Todos are empty - they will be fetched from server using `Cmd` over promise
// Input is also empty
let init(): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = "" }
    let cmd = Cmd.OfPromise.perform getTodos () GotTodos
    model, cmd

// The update function computes the next state of the application
// It does so based on the current state and the incoming message
// It can also run side-effects (encoded as commands) like calling the server via HTTP
// These commands in turn, can dispatch messages to which the update function will react
let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos ->
        { model with Todos = todos }, Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input
        let cmd = Cmd.OfPromise.perform addTodo todo AddedTodo
        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with Todos = model.Todos @ [ todo ] }, Cmd.none

// View takes current model and generates React elements
// It can also use `dispatch` to trigger Msg from UI elements

let view model dispatch =
    div [ Style [ TextAlign TextAlignOptions.Center; Padding 40 ] ] [
        div [ Style [ Display DisplayOptions.InlineBlock ] ] [
            img [ Src "favicon.png" ]
            h1 [] [ str "minimal" ]
            ol [ Style [ MarginRight 20 ] ] [
                for todo in model.Todos ->
                    li [] [ str todo.Description ]
            ]
            input
                [ Value model.Input
                  Placeholder "What needs to be done?"
                  OnChange (fun e -> SetInput e.Value |> dispatch) ]
            button
                [ Disabled (Todo.isValid model.Input |> not)
                  OnClick (fun _ -> dispatch AddTodo) ]
                [ str "Add" ]
        ]
    ]

// In development mode open namepaces for debugging and hot-module replacement
#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

// Following is the entry point for the application - in F# we don't need `main`
Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
