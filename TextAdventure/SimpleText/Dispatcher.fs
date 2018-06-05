module Dispatcher
open Domain
open Combinators

let dispatch command : GamePart =
    fun gamestate ->
        let action = 
            match command with
            | Wait ts -> wait ts
            | Status -> status
            | Exit -> noOp
            | Help -> help
            | NoCommand -> message "Nothing to do."
            | Move dir -> move dir
            | Look -> look
            | StartGame -> message (gamestate.Environment.Description)
            | Undo -> noOp
            | Take itemName -> take itemName
            | Drop itemName -> drop itemName

        {gamestate with LastCommand = command } |> action