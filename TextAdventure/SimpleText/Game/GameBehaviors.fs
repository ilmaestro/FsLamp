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
            | LoseLifeOnUpdate, Some h, None ->
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
            | UseOnExit exitId
            | OpenExit exitId ->
                let exit = Exit.find exitId gamestate.Environment

                gamestate
                |> Environment.openExit exit
            | _ ->
                gamestate

    let outputBehavior f : UpdateGameStateBehavior =
        fun (itemUse, item, gamestate) ->
            match itemUse with
            | GetOutputs ->
                gamestate |> Output.appendOutputs (f item)
            | _ ->
                gamestate

    let addToInventoryBehavior getSuccessOutputs getFailureOutputs: UpdateGameStateBehavior =
        fun (itemuse, item, gamestate) ->
            match itemuse with
            | CanTake true ->
                gamestate
                |> Environment.removeItemFromEnvironment item
                |> Inventory.addItem item
                |> World.updateWorldEnvironment
                |> Output.setOutput (Output (getSuccessOutputs item))
            | _ ->
                gamestate
                |> Output.setOutput (Output (getFailureOutputs item))

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

    // the item can provide light in rooms where no light exists.
    let providesLight =
        //TODO: need something in the gamestate to keep a lightsource state
        ()