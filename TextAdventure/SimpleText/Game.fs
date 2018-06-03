module Game
open Domain
open System

let setInput : Command<Input> =
    fun gamestate input -> Ok {gamestate with Input = input}

let simpleParser : InputParser =
    fun input ->
        match input with
        | "status" -> Status
        | i when i.StartsWith ("wait") ->
            Wait (TimeSpan.FromSeconds(42.))
        | _ -> NoOp

let setEnvironment : GamePart =
    fun gamestate ->
        let environment = 
            match gamestate.Input with
            | Wait ts ->
                {gamestate.Environment with Time = gamestate.Environment.Time + ts}
            | _ -> 
                gamestate.Environment
        {gamestate with Environment = environment}
       
let setOutput : GamePart = 
    fun gamestate ->
        let output =
            match gamestate.Input with
            | Status -> Output (sprintf "%A" gamestate)
            | Wait ts -> Output (sprintf "Waited %i seconds." (int ts.TotalSeconds))
            | NoOp -> Empty
        { gamestate with Output = output }
       
let defaultGamestate =
    { Player = Player "one";
        Health = Health (12.,12.);
        Experience = Experience (0, 1);
        Environment = { Time = DateTime.Parse("1971-01-01 06:01:42")};
        Input = NoOp;
        Output = Empty}

let RunGame (parseInput: InputParser) (handleInput: Command<Input>) (handleState: GamePart) gamestate =
    let rec gameLoop gs =
        Console.Write("\n$?>")
        let result = Console.ReadLine() |> parseInput |> handleInput gs

        match result with
        | Ok gs' ->
            let nextState = gs' |> handleState
            match nextState.Output with
            | Empty -> ()
            | Output s ->
                Console.WriteLine (s)
                gameLoop nextState
        | Error _ -> failwith "Failed to process command"

    gameLoop gamestate