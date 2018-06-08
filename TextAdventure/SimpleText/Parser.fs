module Parser
open Domain
open System

type CommandParser = string -> Command option

let exploreParser : CommandParser =
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
        | [| "undo" |] -> Some Undo
        | [| "take"; itemName |] -> Some (Take itemName)
        | [| "drop"; itemName |] -> Some (Drop itemName)
        | [| "use"; itemName |] -> Some (Use itemName)
        | [| "save" |] -> Some SaveGame
        | _ -> None

let encounterParser : CommandParser =
    fun input ->
        match input.ToLower().Split(" ") with
        | [| "attack" |] -> Some Attack
        | [| "run" |] -> Some Run
        | _ -> None

let mainMenuParser : CommandParser =
    fun input ->
        match input.ToLower().Split(" ") with
        | [| "go" |] -> Some NewGame
        | [| "load" |] -> Some LoadGame
        | _ -> None