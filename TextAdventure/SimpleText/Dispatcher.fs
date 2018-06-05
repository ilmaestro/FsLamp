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
            | NoInput -> noOp
            | Move dir -> move dir

        {gamestate with Input = command } |> action