module Parser
open Domain
open System

type CommandParser = string -> Command option

type Pattern =
| Word of string
| Wildcard

let cleanText (input: string) =
    let trim (s: string) = s.Trim()
    [("?", " "); (".", " "); ("!", " "); (",", " "); ("  ", " ")]
    |> List.fold (fun (acc: string) (key,value) -> 
        acc.Replace(key,value)) (input.ToLower())
    |> trim

let parseText (input: string) : Pattern list =
    input.Split([| ' ' |])
    |> Array.toList
    |> List.map (fun word -> if word = "*" then Wildcard else Word word)

let isQuestion (input: string) =
    input.EndsWith("?")

let makePatterns input =
    input |> cleanText |> parseText

let rec unification pattern input acc =
    match pattern, input with
    | [],[] ->
        acc |> List.rev |> Some
    | Wildcard :: prest, (Word i) :: irest ->
        // option 1: end the wildcard, start a new group
        let continueWithoutWildcard = unification prest irest (Word i :: acc)

        // option 2: continue the wildcard and group
        let continueWithWildcard = unification (Wildcard :: prest) irest (Word i :: acc)

        if continueWithoutWildcard.IsSome then continueWithoutWildcard else continueWithWildcard
    | (Word p) :: prest, (Word i) :: irest  when p = i ->
        unification prest irest acc
    | _ -> None

let extractText (pattern : Pattern list) = 
    pattern 
    |> List.map (fun p ->
        match p with
        | Word w -> w
        | _ -> "")
    |> List.filter (fun x -> x <> "")

// active patterns!
let (|NaturalPattern|_|) pattern input = 
    unification (makePatterns pattern) (makePatterns input) []
    |> Option.map extractText

let parseMatch str =
    match str with
    | NaturalPattern "open the *" openItem -> openItem
    | _ -> []

let exploreParser : CommandParser =
    fun input ->
        match input.ToLower().Trim() with
        | "status" -> Some Status
        | NaturalPattern "wait *" [time] ->
            let (succeeded, result) = Double.TryParse(time)
            if succeeded then 
                Wait (TimeSpan.FromSeconds(result)) |> Some
            else None
        | NaturalPattern "move to the *" [dir]
        | NaturalPattern "go *" [dir]
        | NaturalPattern "move *" [dir] ->
            dir |> Direction.Parse |> Option.map Move
        | "n" -> Some (Move North)
        | "s" -> Some (Move South)
        | "e" -> Some (Move East)
        | "w" -> Some (Move West)
        | "exit" -> Some Exit
        | "help" -> Some Help
        | "look" -> Some Look
        | "undo" -> Some Undo
        | NaturalPattern "take *" [itemName] -> Some (Take itemName)
        | NaturalPattern "drop *" [itemName] -> Some (Drop itemName)
        | NaturalPattern "use *" [itemName] -> Some (Use itemName)
        | "save" -> Some SaveGame
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