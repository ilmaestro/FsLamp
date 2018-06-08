module Domain
open System

[<Measure>] type meter

[<Measure>] type second

let defaultSpeed = 0.1<meter/second>
let time dist : float<second> = dist / defaultSpeed

type Undefined = exn

// primitives
type EnvironmentId = EnvironmentId of int
type ExitId = ExitId of int

// basic player status
type Player = Player of string
type Health = Health of current: float * max: float
type Experience = Experience of total: int * level: int

type InventoryItem =
| InventoryItem of InventoryItemProperties

and InventoryItemProperties = {
    Name: string
    Description: string
    Uses: ItemUse list // list of uses for this item.
}

and ItemUse =
| Unlock of ExitId * Description: string
| Unhide of ExitId * Description: string

type EnvironmentItem =
| EnvironmentItem of EnvironmentItemProperties

and EnvironmentItemProperties = {
    Name: string
    Uses: ItemUse list
}

// the player's immediate location/environment
// environments are connected by paths
type Environment = {
    Id: EnvironmentId
    Name: string
    Description: string
    Exits: Exit list
    InventoryItems: InventoryItem list
    EnvironmentItems: EnvironmentItem list
}

and Exit = {
    Id: ExitId
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
| Use of ItemName: string
| Save

type Output = 
| Output of string list
| Empty

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

// helpers
let timespanFromDistance = function
    | Steps s -> TimeSpan.FromSeconds(float s)
    | Distance d -> TimeSpan.FromSeconds(float (time d))


// constructors
let createEnvironment id name description exits items environmentItems =
    { Id = EnvironmentId id; Name = name; Description = description; Exits = exits; InventoryItems = items; EnvironmentItems = environmentItems }

let createExit id environmentId exitState direction distance description =
    { Id = ExitId id; Target = EnvironmentId environmentId; ExitState = exitState; Direction = direction; 
        Distance = distance; Description = description }

let createInventoryItem name description uses =
    InventoryItem { Name = name; Description = description; Uses = uses }

let createEnvironmentItem name uses =
    EnvironmentItem { Name = name; Uses = uses }

let inventoryItemName item =
    match item with
    | InventoryItem props -> props.Name

let inventoryItemProps item =
    match item with
    | InventoryItem props -> props

let environmentItemDescription item =
    match item with
    | EnvironmentItem props -> props.Name


let tryOpenExit id env =
    env.Exits 
    |> List.tryFind (fun e -> e.Id = id && e.ExitState <> Open)
    |> Option.map (fun e -> { e with ExitState = Open })

let findExit id env =
    env.Exits |> List.find (fun e -> e.Id = id)