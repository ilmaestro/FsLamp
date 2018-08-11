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
| Up
| Down

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
| LookIn of ItemName: string
| Status
| Help
| Exit
| Undo
| Take of ItemName: string
| TakeFrom of TargetName: string * ItemName: string
| Drop of ItemName: string
| Use of ItemName: string
| UseWith of TargetName: string * ItemName: string
| SwitchItemOn of ItemName: string
| SwitchItemOff of ItemName: string
| PutItem of TargetName: string * ItemName: string
| SaveGame
| Read of ItemName: string
// main menu
| NewGame
| LoadGame
// in encounter
| Attack
| Run

type CommandParser = string -> Command option

type Output = 
| Output of string list
| DoNothing
| ExitGame
| Rollback
| GameOver

// extensions
type Direction
with
    static member Parse input =
        match input with
        | "north" -> Some North
        | "south" -> Some South
        | "east" -> Some East
        | "west" -> Some West
        | "up" -> Some Up
        | "down" -> Some Down
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


// computation helpers

type MaybeBuilder() =
    member this.Bind (x, f) =
        match x with
        | Some a -> f a
        | None -> None
    
    member this.Return (x) =
        Some x

let maybe = new MaybeBuilder()