module Parser
open Domain
open System

type CommandParser = string -> Command option

let simpleParser : CommandParser =
    fun input ->
        match input.ToLower().Split(" ") with
        | [| "status" |] -> Some Status
        | [| "wait"; time |] ->
            let (succeeded, result) = Double.TryParse(time)
            if succeeded then 
                Wait (TimeSpan.FromSeconds(result)) |> Some
            else None
        | [|"move"; dir |] ->
            dir |> Direction.Parse |> Option.map Move
        | [| "exit" |] -> Some Exit
        | [| "help" |] -> Some Help
        | [| "look" |] -> Some Look
        | _ -> None