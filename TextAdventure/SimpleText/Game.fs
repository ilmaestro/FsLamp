module Game
open Domain
open System

let defaultGamestate =
    { Player = Player "one";
        Health = Health (12.,12.);
        Experience = Experience (0, 1);
        Environment = { Time = DateTime.Parse("1971-01-01 06:01:42")};
        Input = NoInput;
        Output = Empty}

let RunInConsole (parseInput: InputParser) (dispatcher: Input -> GamePart) gamestate =
    let rec gameLoop gs =
        Console.Write("\n$> ")
        let readline = Console.ReadLine()
        match readline |> parseInput with
        | Some input ->
            let nextState = (input |> dispatcher) <| gs
            match nextState.Output with
            | Empty -> ()
            | Output log ->
                log |> List.iter (printfn "%s")
                gameLoop nextState
        | None ->
            printfn "I don't understand %s." readline
            gameLoop gs

    gameLoop gamestate