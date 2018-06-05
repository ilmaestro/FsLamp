module Game
open Domain
open Combinators
open Parser
open System

let defaultMap =
    [|
        { Id = EnvironmentId 1; 
            Name = "Origin";
            Description = "A moment ago you were just in bed floating above your mind, dreaming about how to add zebras to spreadsheets.  Now it appears you've awakened in a dimlit room. Many unfamiliar smells lurk around you.";
            Paths = [{ 
                        Target = EnvironmentId 2; PathState = Open;
                        Direction = North; Distance = Steps 2; Description = "Creaky Door" }]
        };
        { Id = EnvironmentId 2;
            Name = "Long Hallway, South End"; 
            Description = "The door opens into what appears to be a really long hallway leading North. There's no light at the other end.";
            Paths = [{ 
                        Target = EnvironmentId 1; PathState = Open;
                        Direction = South; Distance = Steps 2; Description = "Creaky Door, leading back where you came from." };
                    { 
                        Target = EnvironmentId 3; PathState = Open;
                        Direction = North; Distance = Steps 6; Description = "Continue down the dark hallway."}
                ]
        };
        { Id = EnvironmentId 3;
            Name = "Long Hallway, North End"; 
            Description = "It gets so dark you have to feel your way around.";
            Paths = [{ 
                        Target = EnvironmentId 2; PathState = Open;
                        Direction = South; Distance = Steps 6; Description = "The south end of the hallway." };
                    { 
                        Target = EnvironmentId 4; PathState = Locked;
                        Direction = East; Distance = Steps 6; Description = "A door with no features, labeled 'Private'."}
                    ]
        };
        { Id = EnvironmentId 4;
            Name = "Office"; 
            Description = "As the door opens, you begin to see the remnants of an old dusty office.  This place hasn't been used in years.";
            Paths = [{ 
                        Target = EnvironmentId 3; PathState = Open;
                        Direction = West; Distance = Steps 6; Description = "The hallway." }]
        }
    |]

let defaultGamestate map =
    { Player = Player "one";
        Health = Health (12.,12.);
        Experience = Experience (0, 1);
        World = { Time = DateTime.Parse("1971-01-01 06:01:42"); Map = map };
        Environment = map.[0];
        LastCommand = NoCommand;
        Output = Empty}


let getCommand (parseInput: CommandParser) =
    Console.Write("\n$> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand

let handleOutput = function
    | Empty -> ()
    | Output log -> log |> List.iter (printfn "%s")

let RunInConsole (parseInput: CommandParser) (dispatcher: Command -> GamePart) initialState =
    let rec gameLoop gamestate history command =
        if command = Exit then ()
        else
            let (nextState, nextHistory) = 
                // crudely rewind history
                if command = Undo then
                    let (oldState, newHistory) =
                        match history with
                        | _ :: old :: tail -> 
                            (old, tail)
                        | _ -> (gamestate, history)

                    ((command |> dispatcher) <| oldState, newHistory)
                else 
                    ((command |> dispatcher) <| gamestate, history)

            nextState.Output |> handleOutput
            
            getCommand parseInput |> gameLoop nextState (nextState :: nextHistory)

    gameLoop initialState [] StartGame