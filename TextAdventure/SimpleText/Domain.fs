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
type MonsterId = MonsterId of int
type ExperiencePoint = int
type ExperienceLevel = int
type AttackPoint = int
type BehaviorId = BehaviorId of int

// basic player status
type Player = Player of string
type Health = Health of current: float * max: float
type Experience = Experience of total: ExperiencePoint * level: ExperienceLevel

type InventoryItem =
| InventoryItem of InventoryItemProperties
| TemporaryItem of InventoryItemProperties * Lifetime: int
| AttackItem of AttackItemProperties

and InventoryItemProperties = {
    Name: string
    Description: string
    Uses: ItemUse list // list of uses for this item.
    Behaviors: BehaviorId list
}

and AttackItemProperties = {
    Name: string
    Description: string
    Damage: int
    Behaviors: BehaviorId list
}

and ItemUse =
| Unlock of ExitId * Description: string
| Unhide of ExitId * Description: string

type EnvironmentItem =
| EnvironmentItem of EnvironmentItemProperties
| Encounter of EncounterProperties
// | Interaction of NPC

and EnvironmentItemProperties = {
    Name: string
    Uses: ItemUse list
}

and EncounterProperties = {
    Description: string
    Monsters: Monster list
    EncounterState: EncounterState
}

and Monster = {
    Id: MonsterId
    Name: string
    Level: ExperienceLevel
    Health: Health
    ExperiencePoints: ExperiencePoint
}

and EncounterState =
| NotStarted
| InProgress
| Complete


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
// open explore
| NoCommand
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
| SaveGame
// main menu
| NewGame
| LoadGame
// in encounter
| Attack
| Run

type Output = 
| Header of string list
| Output of string list
| DoNothing
| ExitGame
| Rollback

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

let isAlive (Health (current, _)) =
    current > 0.