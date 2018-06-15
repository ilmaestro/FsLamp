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
    Behaviors: (Description * ItemUse) list
}

and SwitchState =
| SwitchOn
| SwitchOff

and  ItemUse =
| OpenExit of ExitId // open [exit] with (me)
| UseOnExit of ExitId // use (me) on [exit]
| PutOn of ItemId // put [item] on (me)
| PutIn of ItemId // put [item] in (me)
| TakeFrom of ItemId // take [item] from (me)
| AttackWith of MonsterId // attack [monster] with (me)
| TurnOnOff // turn (me) on/off
| ApplyStats // apply my stats to whom holds me (player or monster)
| LoseLifeOnUpdate
| GetOutputs

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
}

let createInventoryItem id name description health switchState stats behaviors=
    { Id = id; Name = name; Description = description; Health = health; SwitchState = switchState; Stats = stats; Behaviors = behaviors }

let createEnvironmentItem name uses =
    EnvironmentItem { Name = name; Uses = uses }


// All items have all the states optionally, but only specific behaviors can be defined to change those states.
module InventoryWithSlots =
    let AtRuntime () =
        let updateHealthBehavior f =
            fun (item: InventoryItem) ->
                match item.Health with
                | Some h ->
                    { item with Health = Some (f h)}
                | None -> item

        let updateSwitchBehavior f =
            fun (item: InventoryItem) ->
                match item.SwitchState with
                | Some s -> {item with SwitchState = Some (f s)}
                | None -> item

        // let updateBatteryBehavior = addHealthBehavior (Description "Update Battery Power", (updateHealthBehavior (fun (Health(life,total)) -> Health (life - 1, total))))
        // let toggleSwitchBehavior = addSwitchStateBehavior (Description "Toggle On/Off", (updateSwitchBehavior (fun s -> match s with | SwitchOn -> SwitchOff | SwitchOff -> SwitchOn)))



        // let lantern =
        //     { 
        //         Name = "Lantern"; 
        //         Health = Some (Health(10,10));
        //         SwitchState = Some (SwitchOff);
        //         HealthBehaviors =
        //             [ (Description "Update Battery Power", updateBatteryBehavior); ];
        //         SwitchStateBehaviors = 
        //             [ (Description "Toggle On/Off", toggleSwitchBehavior); ];
        //     }
        ()
