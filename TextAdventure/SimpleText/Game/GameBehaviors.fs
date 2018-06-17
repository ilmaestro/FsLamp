module GameBehaviors
open Primitives
open Domain
open ItemUse
open Items
open Environment
open GameState
open Actions

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
                try
                    let exit = Exit.find exitId gamestate.Environment
                    gamestate
                    |> Environment.openExit exit
                    |> Ok
                with
                | _ -> failwithf "couldn't find exit %A" exitId
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
