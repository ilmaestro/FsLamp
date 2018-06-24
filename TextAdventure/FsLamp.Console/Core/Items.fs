module Items
open Primitives
open Domain

type InventoryItem = {
    Id: ItemId
    Name: string
    Description: string
    Health: Health option
    SwitchState: SwitchState option
    Stats: Stats option
    Contains: (InventoryItem list) option
    Behaviors: (Description * ItemUse) list
}

and SwitchState =
| SwitchOn
| SwitchOff

and ItemUse =
| OpenExit of ExitId // open [exit] with (me)
| UseOnExit of ExitId // use (me) on [exit]
| PutOn of InventoryItem option // put [item] on (me)
| PutIn of InventoryItem option // put [item] in (me)
| TakeOut of itemName:string option // take [item] from (me)
| AttackWith of MonsterId // attack [monster] with (me)
| TurnOnOff of SwitchState // turn (me) on/off
| Contains of MaxCount: int
| ApplyStats // apply my stats to whom holds me (player or monster)
| LoseLifeOnUpdate
| LogOutputs
| ProvidesLight
| CanTake of bool
| Readable

type EnvironmentItem =
| Encounter of EncounterProperties
| Interaction

and EncounterProperties = {
    Description: string
    Monsters: Monster list
}

let mutable private inventoryItemCount = 0

let private getNextInventoryItemId () =
    inventoryItemCount <- (inventoryItemCount + 1)
    ItemId (inventoryItemCount)

let createInventoryItem name description health switchState stats contains behaviors =
    { Id = getNextInventoryItemId (); Name = name; Description = description; Health = health; SwitchState = switchState; Stats = stats; Contains = contains; Behaviors = behaviors }
