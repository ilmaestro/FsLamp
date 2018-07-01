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

let rec unification pattern input acc =
    match pattern, input with
    | [],[] ->
        acc |> List.filter (fun (p: Pattern list) -> p.Length > 0) |> List.rev |> Some
    | Wildcard :: prest, (Word i) :: irest ->
        match acc with
        | [] ->
            // option 1: end the wildcard, start a new group
            let continueWithoutWildcard = unification prest irest ([Word i] :: acc)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard and group
                unification pattern irest ([Word i] :: acc)
        | grp :: grpRest ->
            // option 1: end the wildcard, continue group, start a new group
            let continueWithoutWildcard = unification prest irest ([] :: (grp @ [Word i]) :: grpRest)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard, new group
                unification pattern irest ((grp @ [Word i]) :: grpRest)

    | (Word p) :: prest, (Word i) :: irest  when p = i ->
        unification prest irest ([] :: acc)
    | _ -> None

let extractText (pattern : Pattern list list) = 
    pattern 
    |> List.map (fun p1 ->
        p1 
        |> List.map (fun p2 ->
            match p2 with
            | Word w -> w
            | _ -> "")
        |> List.filter (fun x -> x <> ""))

// active patterns!
let (|MatchInput|_|) pattern input = 
    unification (makePatterns pattern) (makePatterns input) []
    |> Option.map extractText

let makeName tokens =
    tokens 
    |> List.filter (fun word -> word <> "the")
    |> String.concat " "

let exploreParser : CommandParser =
    fun input ->
        match input.ToLower().Trim() with
        | MatchInput "wait *" [[time]] ->
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
        | MatchInput "take * from *" [targetTokens; itemTokens] -> 
            (TakeFrom (makeName targetTokens, makeName itemTokens)) |> Some
        | MatchInput "take *" [itemTokens] -> 
            (Take (makeName itemTokens)) |> Some

        | MatchInput "drop *" [itemTokens] -> 
            (Drop (makeName itemTokens)) |> Some

        | MatchInput "use * on *" [itemTokens; targetTokens;]
        | MatchInput "open * with *" [targetTokens; itemTokens;] ->
            (UseWith (makeName targetTokens, makeName itemTokens)) |> Some

        | MatchInput "use *" [itemTokens] -> 
            (Use (makeName itemTokens)) |> Some

        | MatchInput "turn on *" [itemTokens]
        | MatchInput "turn * on" [itemTokens] ->
            (SwitchItemOn (itemTokens |> makeName)) |> Some
        | MatchInput "turn off *" [itemTokens]
        | MatchInput "turn * off" [itemTokens] ->
            (SwitchItemOff (itemTokens |> makeName)) |> Some

        | MatchInput "put * in *" [sourceTokens; itemTokens;] ->
            (PutItem (makeName sourceTokens, makeName itemTokens)) |> Some

        | MatchInput "look in *" [itemTokens] ->
            (LookIn (makeName itemTokens)) |> Some

        | MatchInput "read *" [itemTokens] ->
            (Read (makeName itemTokens)) |> Some
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


let luisParser settings : CommandParser =
    fun input ->
        LUISApi.Client.getResponse settings input
        |> Async.RunSynchronously
        |> Option.bind (fun query ->
            // match the intent
            match query.TopScoringIntent with
            | { Intent = "Move"; Score = s;} when s > 0.5 ->
                match query.Entities with
                | entity :: _ when entity.Type = "Direction" && entity.Score > 0.5 ->
                    entity.Entity |> Direction.Parse |> Option.map Move
                | _ -> None
            | _ ->
                None
        )