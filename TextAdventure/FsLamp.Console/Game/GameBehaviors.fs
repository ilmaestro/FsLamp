module GameBehaviors
open Primitives
open Domain
open ItemUse
open Items
open Environment
open GameState
open Actions
open System

module Common =
    let updateHealthBehavior f : UpdateItemBehavior=
        fun (itemUse: ItemUse, item: InventoryItem) ->
            match itemUse, item.Health, item.SwitchState with
            | LoseLifeOnUpdate, Some h, Some switch ->
                if switch = SwitchOn then { item with Health = Some (f h)} else item
                |> Ok
            | LoseLifeOnUpdate, Some h, None ->
                { item with Health = Some (f h)}
                |> Ok
            | _ -> item |> failItemUpdate "Item use not supported"

    let updateSwitchBehavior : UpdateItemBehavior =
        fun (itemUse: ItemUse, item: InventoryItem) ->
            match itemUse, item.SwitchState with
            | TurnOnOff switchTarget, Some switchSource when switchTarget <> switchSource -> 
                {item with SwitchState = Some switchTarget} |> Ok
            | TurnOnOff switchState, Some _ ->
                item |> failItemUpdate (sprintf "%s is already %A" item.Name switchState )
            | _ -> item |> failItemUpdate "Item use not supported"

    let OpenExitBehavior : UpdateGameStateBehavior =
        fun (itemUse, item, gamestate) ->
            match itemUse with
            | UseOnExit exitId
            | OpenExit exitId ->
                // let exit = Exit.find exitId gamestate.Environment
                match gamestate.Environment.Exits |> List.tryFind (fun e -> e.Id = exitId) with
                | Some exit ->
                    gamestate
                    |> Environment.openExit exit
                    |> Ok
                | None ->
                    gamestate |> failGameStateUpdate "Can't use that here."
            | _ -> 
                gamestate |> failGameStateUpdate "Item use not supported"

    let outputBehavior f : UpdateGameStateBehavior =
        fun (itemUse, item, gamestate) ->
            match itemUse with
            | GetOutputs ->
                gamestate |> Output.appendOutputs (f item) |> Ok
            | _ -> gamestate |> failGameStateUpdate "Item use not supported"

    let addToInventoryBehavior getSuccessOutputs getFailureOutputs: UpdateGameStateBehavior =
        fun (itemuse, item, gamestate) ->
            match itemuse with
            | CanTake true ->
                gamestate
                |> Environment.removeItemFromEnvironment item
                |> Inventory.addItem item
                |> World.updateWorldEnvironment
                |> Output.setOutput (Output (getSuccessOutputs item))
                |> Ok
            | _ ->
                gamestate
                |> Output.setOutput (Output (getFailureOutputs item))
                |> failGameStateUpdate "Item use not supported"

    let putInBehavior: UpdateGameStateBehavior =
        fun (itemUse, item, gamestate) ->
            match itemUse, item.Contains with
            | PutIn (Some itemToPut), Some container ->
                let item' = { item with Contains = Some (itemToPut :: container)}

                match gamestate |> ItemUse.whereIsItem item with
                | Some InInventory ->
                    gamestate
                    |> ItemUse.tryRemoveItemsFromGame (seq { yield itemToPut; yield item; })
                    |> Inventory.addItem item'
                    |> Ok
                | Some InEnvironment ->
                    gamestate
                    |> ItemUse.tryRemoveItemsFromGame (seq { yield itemToPut; yield item; })
                    |> Environment.addItemToEnvironment item'
                    |> Ok
                | None ->
                    gamestate |> failGameStateUpdate "where the hell is the item??"    
            | _ ->
                gamestate |> failGameStateUpdate "can't put that here."

    let takeOutBehavior: UpdateGameStateBehavior =
        fun (itemUse, item, gamestate) ->
            match itemUse, item.Contains with
            | TakeOut (Some itemName), Some container ->
                match container |> List.tryFind (fun i -> i.Name = itemName) with
                | Some itemToTake ->
                    let container' = container |> List.except (seq { yield itemToTake; })
                    let item' = { item with Contains = Some (container')}

                    match gamestate |> ItemUse.whereIsItem item with
                    | Some InInventory ->
                        gamestate
                        |> ItemUse.tryRemoveItemsFromGame (seq { yield item; })
                        |> Inventory.addItem item'
                        |> Inventory.addItem itemToTake
                        |> Ok
                    | Some InEnvironment ->
                        gamestate
                        |> ItemUse.tryRemoveItemsFromGame (seq { yield item; })
                        |> Environment.addItemToEnvironment item'
                        |> Inventory.addItem itemToTake
                        |> Ok
                    | None ->
                        gamestate |> failGameStateUpdate "where the hell is the item??"
                | None ->
                    gamestate |> failGameStateUpdate (sprintf "Couldn't find %s in %s" itemName item.Name)
            | _ ->
                gamestate |> failGameStateUpdate "can't take that here."

module Behaviors =
    open Common
    // some behaviors
    let openExit description exitId =
        ItemUse.addGameStateBehavior 
            (Description description, (Items.OpenExit exitId))
            OpenExitBehavior

    let openSecretPassage description exitId =
        ItemUse.addGameStateBehavior
            (Description description, (Items.UseOnExit exitId))
            OpenExitBehavior

    let loseBattery description amount =
        ItemUse.addItemUseBehavior
            (Description description, Items.LoseLifeOnUpdate)
            (updateHealthBehavior (fun (Health (life,total)) -> Health(life - amount,total)))

    let batteryWarnings description (ranges: (int * int * string) list) =
        ItemUse.addGameStateBehavior
            (Description description, Items.GetOutputs)
            (outputBehavior (fun item -> 
                match item.Health with
                | Some (Health(life, _)) ->
                    match ranges |> List.tryFind (fun (min, max, _) -> min <= life && life <= max) with
                    | Some (_,_,output) -> [output]
                    | None -> []
                | None -> []    
            ))

    let takeItem description canTake =
        ItemUse.addGameStateBehavior
            (Description description, Items.CanTake canTake)
            (addToInventoryBehavior (fun item -> [sprintf "%s taken." item.Name]) (fun _ -> [sprintf "%s" description]))

    let turnOnOff description =
        ItemUse.addItemUseBehavior
            (Description description, ItemUse.Defaults.TurnOnOff)
            (updateSwitchBehavior)

    let putIn description =
        ItemUse.addGameStateBehavior
            (Description description, ItemUse.Defaults.PutIn)
            (putInBehavior)

    let takeOut description =
        ItemUse.addGameStateBehavior
            (Description description, ItemUse.Defaults.TakeOut)
            (takeOutBehavior)

        