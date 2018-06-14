module Items
open Primitives
open Domain


// A Type specific implementation.  This works, but requires the game to have to know what to do 
// with a Lantern object and when to call its functions.
module Lantern =
    type Lantern = {
        Name: string
        Keywords: string list
        Battery: Health
        SwitchState: SwitchState
    }
    and SwitchState =
    | SwitchOn
    | SwitchOff

    let decrementBattery amount lantern =
        let (Health(life,total)) = lantern.Battery
        let newlife = life - amount
        { lantern with Battery = Health((if newlife < 0 then 0 else newlife), total)}

    let setSwitch switch lantern =
        { lantern with SwitchState = switch }

    let toggleSwitch lantern =
        match lantern.SwitchState with
        | SwitchOn -> lantern |> setSwitch SwitchOff
        | SwitchOff -> lantern |> setSwitch SwitchOn
    

// this attempts to store state in the ItemUse DU and seperate behaviors via entity (BehaviorId), but
//  this creates a lot of complexity around updating the ItemUse state, which is part of the Behaviors 
//  list of the InventoryItem
module InventoryWithBehaviors =
    type InventoryItem = {
        Name: string
        Keywords: string list
        // TODO; stateful data?
        Behaviors: (ItemUse * Description * BehaviorId) list // pair uses with behavior entities
    }

    and ItemUse =
    // | ModifyExit of ExitId
    // | ModifyHealth of Health
    // | ModifyDamage of Damage
    // | ModifyAttack of AttackStat
    // | ModifyDefense of DefenseStat
    | KeepHealth of Health
    | SwitchOnOff of bool

    let AtRuntime () =
        let lantern = 
            { Name = "Lantern";
                Keywords = ["Lantern of Power"; "Torch"; "Thing that produces light"];
                Behaviors = 
                    [
                        (KeepHealth (Health(15,15)), Description "Battery Power", BehaviorId 1)
                    ]}

        ()


// All items have all the states optionally, but only specific behaviors can be defined to change those states.
module InventoryWithSlots =
    type HealthBehaviorId = HealthBehaviorId of int 
    type SwitchStateBehaviorId = SwitchStateBehaviorId of int
    type InventoryItem = {
        Name: string
        Health: Health option
        SwitchState: SwitchState option
        HealthBehaviors: (Description * HealthBehaviorId) list
        SwitchStateBehaviors: (Description * SwitchStateBehaviorId) list
    }
    and SwitchState =
    | SwitchOn
    | SwitchOff

    type UpdateBehavior = (Description * (InventoryItem -> InventoryItem))

    let private healthBehaviorCache : Map<HealthBehaviorId,UpdateBehavior> ref = ref Map.empty

    let addHealthBehavior b =
        let id = HealthBehaviorId ((!healthBehaviorCache).Count + 1)
        healthBehaviorCache := (!healthBehaviorCache).Add(id, b)
        id

    let private switchStateBehaviorCache : Map<SwitchStateBehaviorId,UpdateBehavior> ref = ref Map.empty

    let addSwitchStateBehavior b =
        let id = SwitchStateBehaviorId ((!switchStateBehaviorCache).Count + 1)
        switchStateBehaviorCache := (!switchStateBehaviorCache).Add(id, b)
        id

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

        let updateBatteryBehavior = addHealthBehavior (Description "Update Battery Power", (updateHealthBehavior (fun (Health(life,total)) -> Health (life - 1, total))))
        let toggleSwitchBehavior = addSwitchStateBehavior (Description "Toggle On/Off", (updateSwitchBehavior (fun s -> match s with | SwitchOn -> SwitchOff | SwitchOff -> SwitchOn)))



        let lantern =
            { 
                Name = "Lantern"; 
                Health = Some (Health(10,10));
                SwitchState = Some (SwitchOff);
                HealthBehaviors =
                    [ (Description "Update Battery Power", updateBatteryBehavior); ];
                SwitchStateBehaviors = 
                    [ (Description "Toggle On/Off", toggleSwitchBehavior); ];
            }
        ()
