module Game
open Domain
open Combinators
open Parser
open System

let defaultMap =
    [|
        (createEnvironment 1 "Origin"
            "A moment ago you were just in bed floating above your mind, dreaming about how to add zebras to spreadsheets.  Now it appears you've awakened in a dimlit room. Many unfamiliar smells lurk around you."
            [createExit 2 Open North (Steps 2) "Creaky Door"]
            [createItem "RustyKey"]
        );
        (createEnvironment 2 "Long Hallway, South End"
            "The door opens into what appears to be a really long hallway leading North. There's no light at the other end."
            [
                createExit 1 Open South (Steps 2) "Creaky Door, leading back where you came from.";
                createExit 3 Open North (Steps 6) "Continue down the dark hallway.";
                ]
            []
        );
        (createEnvironment 3 "Long Hallway, North End"
            "It gets so dark you have to feel your way around."
            [ 
                createExit 2 Open South (Steps 6) "The south end of the hallway.";
                createExit 4 Locked East (Steps 6) "A door with no features, labeled 'Private'."
            ]
            []
        );
        (createEnvironment 4 "Office"
            "As the door opens, you begin to see the remnants of an old dusty office.  This place hasn't been used in years."
            [ createExit 3 Open West (Steps 6) "The hallway."]
            []
        );
    |]

let defaultGamestate map =
    { Player = Player "one";
        Health = Health (12.,12.);
        Experience = Experience (0, 1);
        Inventory = [];
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