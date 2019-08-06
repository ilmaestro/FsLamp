module GameBehaviors
open FsLamp.Core
open FsLamp.Core.Primitives
open FsLamp.Core.Domain
open FsLamp.Core.ItemUse
open FsLamp.Core.Items
open FsLamp.Core.GameState
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

    let updateSwitchBehavior : UpdateGameStateBehavior =
        fun (itemUse: ItemUse, item, gamestate) ->
            match itemUse, item.SwitchState with
            | TurnOnOff switchTarget, Some switchSource when switchTarget <> switchSource -> 
                gamestate
                |> Inventory.updateItem {item with SwitchState = Some switchTarget}
                |> Ok
            | TurnOnOff switchState, Some _ ->
                gamestate |> failGameStateUpdate (sprintf "%s is already %s" item.Name (switchState.ToString()) )
            | _ -> gamestate |> failGameStateUpdate "Item use not supported"

    let slidesSwitchBehavior : UpdateGameStateBehavior =
        let rec getCommand () = 
            let keyInfo = System.Console.ReadKey()
            match keyInfo.Key with
            | ConsoleKey.UpArrow -> "start"
            | ConsoleKey.DownArrow -> "end"
            | ConsoleKey.RightArrow -> "next"
            | ConsoleKey.LeftArrow -> "back"
            | ConsoleKey.Escape -> "exit"
            | _ -> getCommand ()

        fun (itemUse: ItemUse, item, gamestate) ->
            // get slide
            let slideContent = Utility.readTextAsset(item.Description)
            let slides = slideContent.Split("---")
            //let mutable slideIndex = 0
            let slideLength = slides.Length

            // interupt the game loop with our own game loop
            let rec slideLoop slideIndex =
                ConsoleService.clearScreen()
                printfn ""
                ConsoleService.Markdown.renderSomething slides.[slideIndex]
                printfn ""
                printf "%i/%i" (slideIndex + 1) (slideLength)

                match getCommand() with
                | "start" ->
                    slideLoop 0
                | "next" ->
                    let nextIndex = if slideIndex < slideLength - 1 then (slideIndex + 1) else slideIndex
                    slideLoop nextIndex
                | "back" ->
                    let nextIndex = if slideIndex > 0 then (slideIndex - 1) else slideIndex
                    slideLoop nextIndex
                | "end" ->
                    slideLoop (slideLength - 1)
                | _ -> ()

            slideLoop 0
            gamestate |> Ok

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
            | LogOutputs ->
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
                    // check if item is take-able
                    match itemToTake |> ItemUse.tryFindItemUse (ItemUse.Defaults.CanTake) with
                    | Some (Description desc, _) ->
                        let container' = container |> List.except (seq { yield itemToTake; })
                        let item' = { item with Contains = Some (container')}

                        match gamestate |> ItemUse.whereIsItem item with
                        | Some InInventory ->
                            gamestate
                            |> ItemUse.tryRemoveItemsFromGame (seq { yield item; })
                            |> Inventory.addItem item'
                            |> Inventory.addItem itemToTake
                            |> Output.setOutput (Output [desc])
                            |> Ok
                        | Some InEnvironment ->
                            gamestate
                            |> ItemUse.tryRemoveItemsFromGame (seq { yield item; })
                            |> Environment.addItemToEnvironment item'
                            |> Inventory.addItem itemToTake
                            |> Output.setOutput (Output [desc])
                            |> Ok
                        | None ->
                            gamestate |> failGameStateUpdate "where the hell is the item??"
                    | None ->
                        gamestate |> failGameStateUpdate "it doesn't seem to want to move."
                    
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
            (Description description, Items.LogOutputs)
            (outputBehavior (fun item -> 
                match item.Health with
                | Some (Health(life, _)) ->
                    match ranges |> List.tryFind (fun (min, max, _) -> min <= life && life <= max) with
                    | Some (_,_,output) -> [output]
                    | None -> []
                | None -> []    
            ))

    let theWubOutput =
        let mutable text = 
            (Utility.readTextAsset "story.md").Split([|'\n'|]) 
            |> Array.toList
            |> List.filter (fun x -> x.Length > 0)

        ItemUse.addGameStateBehavior
            (Description "The Wub", Items.ReadOnUpdate)
            (fun (_, _, gamestate) -> 
                match text with
                | [] -> gamestate |> Ok
                | head :: tail ->
                    text <- tail
                    gamestate
                    |> Output.prependOutputs [sprintf "The Wub: **%s**" head; "***";]
                    |> Ok
                )

    let takeItem description canTake =
        ItemUse.addGameStateBehavior
            (Description description, Items.CanTake canTake)
            (addToInventoryBehavior (fun item -> [sprintf "%s taken." item.Name]) (fun _ -> [sprintf "%s" description]))

    let turnOnOff description =
        ItemUse.addGameStateBehavior
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

    let slidesOnOff description =
        ItemUse.addGameStateBehavior
            (Description description, ItemUse.Defaults.TurnOnOff)
            (slidesSwitchBehavior)