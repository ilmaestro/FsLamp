module Domain
open System

[<Measure>] type meter

[<Measure>] type second

let defaultSpeed = 0.1<meter/second>
let time dist : float<second> = dist / defaultSpeed

type Undefined = exn

// basic player status
type Player = Player of string
type Health = Health of current: float * max: float
type Experience = Experience of total: int * level: int

// the player's immediate location/environment
// environments are connected by paths
type Environment = {
    Id: EnvironmentId
    Name: string
    Paths: Path list
}
and EnvironmentId = EnvironmentId of int
and Path = {
    Source: EnvironmentId
    Target: EnvironmentId
    Direction: Direction
    Distance: Distance
}
and Direction =
| North
| South
| East
| West
and Distance = 
| Steps of int // assuming 1 step per second
| Distance of float<meter> // assuming 0.1 meters per second

type World = {
    Time: DateTime
    Map: Map
}
and Map = Environment []

let timespanFromDistance = function
    | Steps s -> TimeSpan.FromSeconds(float s)
    | Distance d -> TimeSpan.FromSeconds(float (time d))

type Input =
| NoInput
| Wait of TimeSpan
| Move of Direction
| Look
| Status
| Help
| Exit

type Output = 
| Output of string list
| Empty

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Environment: Environment
    World: World
    Input: Input
    Output: Output
}

type GameError = Undefined

type Command<'a> = GameState -> 'a -> Result<GameState, GameError>

type GamePart = GameState -> GameState
type InputParser = string -> Input option


// extensions
type Direction
with
    static member Parse input =
        match input with
        | "north" -> Some North
        | "south" -> Some South
        | "east" -> Some East
        | "west" -> Some West
        | _ -> None