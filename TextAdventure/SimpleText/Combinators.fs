module Combinators
open Domain

let wait ts : GamePart =
    fun gamestate ->
        let world = {gamestate.World with Time = gamestate.World.Time + ts}
        let outputs = [
            sprintf "Waited %i second(s)." (int ts.TotalSeconds);
            sprintf "The time is now %A." world.Time;
        ]

        { gamestate with 
                World = world
                Output = Output outputs }

let status : GamePart =
    fun gamestate ->
        let outputs = [
            sprintf "%A" gamestate.Player;
            sprintf "%A" gamestate.Health;
            sprintf "%A" gamestate.Experience;
            sprintf "%A" gamestate.World.Time;            
        ]
        { gamestate with Output = Output outputs }

let help : GamePart =
    fun gamestate ->
        let help = """
Commands:
status                        - get the current player status
wait {seconds}                - wait for specified number of seconds
move {north|south|east|west}  - move in specified direction
help                          - show help
look                          - find things
exit                          - exit the game
        """
        let outputs = [help]
        { gamestate with Output = Output outputs }


let noOp : GamePart =
    fun gamestate ->
        { gamestate with Output = Empty }

let move dir: GamePart =
    fun gamestate ->
        let pathOption = gamestate.Environment.Paths |> List.tryFind (fun p -> p.Direction = dir && p.PathState.IsVisible)
        let nextEnvironment = 
            match pathOption with
            | Some path ->
                gamestate.World.Map |> Array.find (fun env -> env.Id = path.Target)
            | None ->
                gamestate.Environment

        // update the world and environment
        match pathOption with
        | Some path ->
            // calculate and add the travel time 
            let time = path.Distance |> timespanFromDistance
            let world = {gamestate.World with Time = gamestate.World.Time + time }

            // log outputs
            let log = [path.Description; nextEnvironment.Description]
            { gamestate with Environment = nextEnvironment; World = world; Output = Output log }
        | None ->
            { gamestate with Output = Output [sprintf "There are no paths to the %A." dir] }

let look : GamePart =
    fun gamestate ->
        // list the paths
        let pathHelper = sprintf "To the %A: %s"
        let paths = gamestate.Environment.Paths |> List.map (fun p -> pathHelper p.Direction p.Description)
        { gamestate with Output = Output paths }