module Parser
open Domain
open System

let simpleParser : InputParser =
    fun input ->
        match input.ToLower().Split(" ") with
        | [| "status" |] -> Some Status
        | [| "wait"; time |] ->
            let (succeeded, result) = Double.TryParse(time)
            if succeeded then 
                Wait (TimeSpan.FromSeconds(result)) |> Some
            else None
        | [| "exit" |] -> Some Exit
        | [| "help" |] -> Some Help
        | _ -> None