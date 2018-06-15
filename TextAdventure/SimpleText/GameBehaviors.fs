module GameBehaviors
open Domain
open ItemUse
open Items
open Environment
open GameState

// module Inventory =

    // let private cache : Map<BehaviorId, GameBehavior<InventoryItem>> ref = ref Map.empty

    // let add b =
    //     let id = BehaviorId ((!cache).Count + 1)
    //     cache := (!cache).Add(id, b)
    //     id

    // let find id =
    //     (!cache).TryFind(id)

    // let decrementLifeOnUpdateBehavior = 
    //     UpdateBehavior (
    //         fun (item: InventoryItem) gs ->
    //             match item with
    //             | TemporaryItem (props, life) ->
    //                 let newItem = TemporaryItem (props, life - 1)
    //                 (newItem, gs)
    //             | _ ->
    //                 (item, gs))

    // let rangedOutputBehavior (ranges: (int * int * string) list) =
    //     OutputBehavior (
    //         fun (item: InventoryItem) _ ->
    //             match item with
    //             | (TemporaryItem (_, life)) ->
    //                 match ranges |> List.tryFind (fun (min, max, _) -> min <= life && life <= max) with
    //                 | Some (_,_,output) -> [output]
    //                 | None -> []
    //             | _ -> []
    //     )
    

module ItemUses =
    // let private cache : Map<BehaviorId, GameBehavior<InventoryItem * ItemUse>> ref = ref Map.empty

    // let add b =
    //     let id = BehaviorId ((!cache).Count + 1)
    //     cache := (!cache).Add(id, b)
    //     id

    // let find id =
    //     (!cache).TryFind(id)

    // let toggleOnOffBehavior =
    //     UpdateBehavior (
    //         fun (item: InventoryItem, usage: ItemUse) gs ->
    //             match item, usage with
    //             | OnOffItem (props, onOff), Switch OnOff ->
    //                 ((OnOffItem (props, not onOff), usage), gs)
    //             | _ -> ((item, usage), gs)
    //     )

    let updateHealthBehavior f : UpdateItemBehavior=
        fun (itemUse: ItemUse, item: InventoryItem) ->
            match itemUse, item.Health with
            | LoseLifeOnUpdate, Some h ->
                { item with Health = Some (f h)}
            | _ -> item

    let updateSwitchBehavior f : UpdateItemBehavior=
        fun (itemUse: ItemUse, item: InventoryItem) ->
            match itemUse, item.SwitchState with
            | TurnOnOff, Some s -> {item with SwitchState = Some (f s)}
            | _ -> item

    let OpenExitBehavior : UpdateGameStateBehavior =
        fun (itemUse, _, gamestate) ->
            match itemUse with
            | OpenExit exitId ->
                let exit = Exit.find exitId gamestate.Environment

                gamestate
                |> Environment.openExit exit
            | _ ->
                gamestate