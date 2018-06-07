module Dispatcher
open Domain
open Combinators
open GameState

let getSaveDataFilename = 
    "./SaveData/GameSave.json"

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
            | Use itemName -> useItem itemName
            | Save -> save getSaveDataFilename

        {gamestate with LastCommand = command } |> action