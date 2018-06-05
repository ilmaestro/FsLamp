module Game
open Domain
open System

let defaultMap =
    [|
        { Id = EnvironmentId 1; 
            Name = "Room 1"; 
            Paths = [{Source = EnvironmentId 1; Target = EnvironmentId 2; Direction = North; Distance = Steps 2 }]
        };
        { Id = EnvironmentId 2;
            Name = "Room 2"; 
            Paths = [{Source = EnvironmentId 2; Target = EnvironmentId 1; Direction = South; Distance = Steps 2 }]
        }
    |]

let defaultGamestate map =
    { Player = Player "one";
        Health = Health (12.,12.);
        Experience = Experience (0, 1);
        World = { Time = DateTime.Parse("1971-01-01 06:01:42"); Map = map };
        Environment = map.[0];
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