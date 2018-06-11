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

let makePatterns input =
    input |> cleanText |> parseText

// acc = [[pattern list] list]: each list corresponds to a Wildcard group

let rec unification pattern input acc =
    match pattern, input with
    | [],[] ->
        acc |> List.rev |> Some
    | Wildcard :: prest, (Word i) :: irest ->
        match acc with
        | [] ->
            // option 1: end the wildcard, start a new group
            let continueWithoutWildcard = unification prest irest ([Word i)] :: acc)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard and group
                unification pattern irest ([Word i)] :: acc)
        | grp :: grpRest ->
            // option 1: end the wildcard, continue group, start a new group
            let continueWithoutWildcard = unification prest irest ([] :: (grp @ [Word i]) :: grpRest)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard, new group
                unification pattern irest ((grp @ [Word i]) :: grpRest)

    | (Word p) :: prest, (Word i) :: irest  when p = i ->
        unification prest irest acc
    | _ -> None

let matchPattern (pattern : Sentence) (input : Sentence) = 
    if pattern.IsQuestion <> input.IsQuestion then None
    else unification pattern.Contents input.Contents []
(matchPattern (makeSentence "open * with * or *") (makeSentence "open the bork creaky door with rusty key or the snarky wrench"))


let extractText (pattern : Pattern list list) = 
    pattern 
    |> List.map (fun p1 ->
        p1 
        |> List.filter (fun p2 -> p2.Length > 0) // ignore empty lists
        |> List.map (fun p2 ->
            match p2 with
            | Word w -> w
            | _ -> "")
        |> List.filter (fun x -> x <> ""))

// active patterns!
let (|MatchInput|_|) pattern input = 
    unification (makePatterns pattern) (makePatterns input) []
    |> Option.map extractText

let parseMatch str =
    match str with
    | MatchInput "open the *" openItem -> openItem
    | _ -> []

let exploreParser : CommandParser =
    fun input ->
        match input.ToLower().Trim() with
        | MatchInput "wait *" [time] ->
            let (succeeded, result) = Double.TryParse(time)
            if succeeded then 
                Wait (TimeSpan.FromSeconds(result)) |> Some
            else None
        // Move commands
        | MatchInput "move to the *" [[dir]]
        | MatchInput "go to the *" [[dir]]
        | MatchInput "go *" [[dir]]
        | MatchInput "move *" [[dir]] ->
            dir |> Direction.Parse |> Option.map Move
        | "n" -> Some (Move North)
        | "s" -> Some (Move South)
        | "e" -> Some (Move East)
        | "w" -> Some (Move West)
        // Item commands
        | MatchInput "take *" [[itemName]] -> Some (Take itemName)
        | MatchInput "drop *" [[itemName]] -> Some (Drop itemName)
        | MatchInput "use *" [[itemName]] -> Some (Use itemName)
        | MatchInput "open * with *" [targetNames; itemNames;] ->
            Some (Use "TODO")
        // single word commands
        | "status" -> Some Status
        | "exit" -> Some Exit
        | "help" -> Some Help
        | "look" -> Some Look
        | "undo" -> Some Undo
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