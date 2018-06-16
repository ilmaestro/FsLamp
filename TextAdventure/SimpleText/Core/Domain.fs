module Domain
open System
open Primitives

type Stats = {
    Attack: AttackStat
    Defense: DefenseStat
    Damage: Damage
}

type Player = {
    Name: string
    Health: Health
    Experience: Experience
    Stats: Stats
}

type Monster = {
    Id: MonsterId
    Name: string
    Health: Health
    ExperiencePoints: ExperiencePoint
    Stats: Stats
}

// type InventoryItem =
// | InventoryItem of InventoryItemProperties
// | TemporaryItem of InventoryItemProperties * Lifetime: int
// | OnOffItem of InventoryItemProperties * OnOff: bool
// | AttackItem of AttackItemProperties

// and InventoryItemProperties = {
//     Name: string
//     Description: string
//     Uses: ItemUse list // list of uses for this item.
//     Behaviors: BehaviorId list
// }

// and AttackItemProperties = {
//     Name: string
//     Description: string
//     Damage: int
//     Behaviors: BehaviorId list
// }

// and ItemUse =
// | Unlock of ExitId * Description: string
// | Unhide of ExitId * Description: string
// | Put of PutUse * id: string
// | Contain of ContainUse
// | Switch of SwitchType

// and PutUse =
// | PutOn
// | PutIn

// and ContainUse =
// | ContainOn
// | ConainIn

// and SwitchType =
// | OnOff


// the player's immediate location/environment
// environments are connected by paths
type Exit = {
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
    (int current) > 0

let damage (Damage d) (Health (life, total)) =
    let newLife = if life - d > 0 then (life - d) else 0
    Health (newLife, total)

let power (AttackStat attack) (Damage damage) =
    Power (attack + damage)
   
let attackRoll (DefenseStat targetDefense) (Power sourcePower) (roll: unit -> int) =
    (roll() + sourcePower) >= targetDefense

let healthDescription (Health (life, total)) =
    sprintf "%i/%i" life total

let createStats attack defense damage =
    { Attack = (AttackStat attack); Defense = DefenseStat defense; Damage = Damage damage }
