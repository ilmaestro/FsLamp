module Combinators
open Domain

let wait ts : GamePart =
    fun gamestate ->
        let environment = {gamestate.Environment with Time = gamestate.Environment.Time + ts}
        let outputs = [
            sprintf "Waited %i second(s)." (int ts.TotalSeconds);
            sprintf "The time is now %A." environment.Time;
        ]

        { gamestate with 
                Environment = environment
                Output = Output outputs }

let status : GamePart =
    fun gamestate ->
        let outputs = [
            sprintf "%A" gamestate.Player;
            sprintf "%A" gamestate.Health;
            sprintf "%A" gamestate.Experience;
            sprintf "%A" gamestate.Environment.Time;            
        ]
        { gamestate with Output = Output outputs }

let help : GamePart =
    fun gamestate ->
        let help = """
Commands:
status
wait {seconds}
help
exit
        """
        let outputs = [help]
        { gamestate with Output = Output outputs }


let noOp : GamePart =
    fun gamestate ->
        { gamestate with Output = Empty }
