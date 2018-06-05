module Dispatcher
open Domain
open Combinators

let dispatch input : GamePart =
    fun gamestate ->
        let app = 
            match input with
            | Wait ts -> wait ts
            | Status -> status
            | Exit -> noOp
            | Help -> help
            | NoInput -> noOp
        {gamestate with Input = input } |> app