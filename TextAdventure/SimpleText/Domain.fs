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

type Item =
| Item of ItemProperties

and ItemProperties = {
    Name: string
}

// the player's immediate location/environment
// environments are connected by paths
type Environment = {
    Id: EnvironmentId
    Name: string
    Description: string
    Exits: Exit list
    Items: Item list
}

and EnvironmentId = EnvironmentId of int

and Exit = {
    Target: EnvironmentId
    Direction: Direction
    Distance: Distance
    Description: string
    ExitState: ExitState
}

and Direction =
| North
| South
| East
| West

and Distance = 
| Steps of int // assuming 1 step per second
| Distance of float<meter> // assuming 0.1 meters per second

and ExitState =
| Open
| Locked
| Hidden

type World = {
    Time: DateTime
    Map: Map
}
and Map = Environment []

let timespanFromDistance = function
    | Steps s -> TimeSpan.FromSeconds(float s)
    | Distance d -> TimeSpan.FromSeconds(float (time d))

type Command =
| NoCommand
| StartGame
| Wait of TimeSpan
| Move of Direction
| Look
| Status
| Help
| Exit
| Undo
| Take of ItemName: string
| Drop of ItemName: string

type Output = 
| Output of string list
| Empty

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Inventory: Item list
    Environment: Environment
    World: World
    LastCommand: Command
    Output: Output
}

type GameHistory = GameState list

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
 
type ExitState
with
    member x.IsVisible =
        match x with
        | Hidden -> false
        | _ -> true

// constructors

let createEnvironment id name description exits items =
    { Id = EnvironmentId id; Name = name; Description = description; Exits = exits; Items = items }

let createExit environmentId exitState direction distance description =
    { Target = EnvironmentId environmentId; ExitState = exitState; Direction = direction; 
        Distance = distance; Description = description }

let createItem name =
    Item { Name = name }

let itemDescription item =
    match item with
    | Item props -> props.Name