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
status
wait {seconds}
move {north|south|east|west}
help
exit
        """
        let outputs = [help]
        { gamestate with Output = Output outputs }


let noOp : GamePart =
    fun gamestate ->
        { gamestate with Output = Empty }

let move dir: GamePart =
    fun gamestate ->
        let (path, environment) = 
            match gamestate.Environment.Paths |> List.tryFind (fun p -> p.Direction = dir) with
            | Some path ->
                (Some path, gamestate.World.Map |> Array.find (fun env -> env.Id = path.Target))
            | None ->
                (None, gamestate.Environment)

        // update the world and environment
        match path with
        | Some p ->
            // calculate and add the travel time 
            let world = {gamestate.World with Time = gamestate.World.Time + (p.Distance |> timespanFromDistance)}
            // log outputs
            let log = [
                (sprintf "You have entered %s." environment.Name)
            ]
            { gamestate with Environment = environment; World = world; Output = Output log }
        | None ->
            { gamestate with Output = Output [sprintf "There are no paths to the %A." dir] }