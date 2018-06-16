module Actions
open Primitives
open Domain
open GameState
open Environment
open Player
open System
open ItemUse

type GamePart = GameState -> GameState

[<AutoOpen>]
module Common =
    let status : GamePart =
        fun gamestate ->
            let outputs = [
                sprintf "%A" gamestate.Player;
                sprintf "%A" gamestate.World.Time;    
                sprintf "%A" gamestate.Inventory;
            ]
            { gamestate with Output = Output outputs }
    let noOp : GamePart =
        fun gamestate ->
            { gamestate with Output = DoNothing }
    let message s : GamePart =
        fun gamestate ->
            {gamestate with Output = Output [s]}

    let tryUseItemFromInventory (itemName: string) itemUse gamestate =
            let itemOption = 
                gamestate.Environment.InventoryItems 
                |> List.tryFind (fun i ->
                    (i.Name.ToLower()) = itemName.ToLower())

            match itemOption with
            | Some item ->
                let takeableUse = 
                    item 
                    |> tryFindItemUse itemUse //(ItemUse.Defaults.CanTake) 
                    |> Option.map (fun itemUse -> (itemUse, findGameStateBehavior itemUse))

                match takeableUse with
                | Some ((_, itemUse), Some update) ->
                    Ok (update (itemUse, item, gamestate))
                | _ ->
                    Error Items.CantUse
            | None ->
                Error Items.CantFind

module Explore =
    let wait ts : GamePart =
        fun gamestate ->
            let world = {gamestate.World with Time = gamestate.World.Time + ts}
            let outputs = [
                sprintf "Waited %i second(s)." (int ts.TotalSeconds);
                sprintf "The time is now %A." world.Time;
            ]

            { gamestate with 
                    World = world
                    Output = Output outputs }


    let help : GamePart =
        fun gamestate ->
            let help = """
    Commands:
    status                        - get the current player status
    wait {seconds}                - wait for specified number of seconds
    move {north|south|east|west}  - move in specified direction
    help                          - show help
    look                          - find things
    exit                          - exit the game
            """
            let outputs = [help]
            { gamestate with Output = Output outputs }


    let exitGame : GamePart =
        fun gamestate ->
            { gamestate with Output = ExitGame }

    let move dir: GamePart =
        fun gamestate ->
            let exitOption = gamestate.Environment.Exits |> List.tryFind (fun p -> p.Direction = dir)
            match exitOption with
            | Some exit ->
                match exit.ExitState with
                | Open ->
                    let nextEnvironment = Environment.findById exit.Target gamestate
                    gamestate
                    |> World.updateWorldTravelTime exit.Distance
                    |> setEnvironment nextEnvironment
                    |> Output.setOutput (Output [nextEnvironment.Description])
                    |> Encounter.checkEncounter
                | Locked ->
                    gamestate |> Output.setOutput (Output ["The exit is locked."])
                | Hidden -> 
                    gamestate |> Output.setOutput (Output [sprintf "There are no exits to the %A." dir])
            | None ->
                gamestate |> Output.setOutput (Output [sprintf "There are no exits to the %A." dir])

    let look : GamePart =
        fun gamestate ->
            let exitHelper = sprintf "\tA %s to the %A"
            let itemHelper = sprintf "\tA %s %s"
            let exits = gamestate.Environment.Exits |> List.filter (fun e -> e.ExitState <> Hidden) |> List.map (fun p -> exitHelper p.Description p.Direction)
            let items = gamestate.Environment.InventoryItems |> List.map (fun {Name = name; Description = description } -> itemHelper name description)
            let log = [
                yield gamestate.Environment.Description
                yield "Exits"; yield! exits; 
                match items with [] -> () | _ -> yield ""; yield "You see"; yield! items ]
            { gamestate with Output = Output log }

    let take (itemName: string) : GamePart =
        fun gamestate ->
            // find item
            let itemOption = 
                gamestate.Environment.InventoryItems 
                |> List.tryFind (fun i ->
                    (i.Name.ToLower()) = itemName.ToLower())

            match itemOption with
            | Some item ->
                let takeableUse = 
                    item 
                    |> tryFindItemUse (ItemUse.Defaults.CanTake) 
                    |> Option.map (fun itemUse -> (itemUse, findGameStateBehavior itemUse))

                match takeableUse with
                | Some ((_, itemUse), Some update) ->
                    match update (itemUse, item, gamestate) with
                    | Ok gs -> gs
                    | Error failure -> 
                        failure.GameState
                | _ ->
                    gamestate
                    |> Output.setOutput (Output [sprintf "You try to take %s, but it own't budge." item.Name])
            | None ->
                let output = [sprintf "Couldn't find %s." itemName]
                {gamestate with Output = Output output }

    let drop (itemName: string) : GamePart =
        fun gamestate ->
            // find item
            let itemOption = 
                gamestate.Inventory
                |> List.tryFind (fun i -> (i.Name.ToLower()) = itemName.ToLower())
            match itemOption with
            | Some item ->
                gamestate
                |> Environment.addItemToEnvironment item
                |> World.updateWorldEnvironment
                |> Output.setOutput (Output [sprintf "You dropped %s." item.Name])
            | None ->    
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    let switch (itemName: string) switchState : GamePart =
        fun gamestate ->
            // find item
            let itemOption = 
                gamestate.Inventory
                |> List.tryFind (fun i ->
                    (i.Name.ToLower()) = itemName.ToLower())

            match itemOption with
            | Some item ->
                // find use
                let switchableUse = 
                    item 
                    |> tryFindItemUse (ItemUse.Defaults.TurnOnOff) 
                    |> Option.map (fun itemUse -> (itemUse, findItemUseBehavior itemUse))

                match switchableUse with
                | Some (_, Some update) ->
                    // try to update
                    match update (Items.TurnOnOff switchState, item) with
                    | Ok item ->
                        gamestate 
                        |> Inventory.updateItem item
                        |> Output.setOutput (Output [sprintf "%s turned %A" item.Name switchState])
                    | Error failure ->
                        gamestate
                        |> Output.setOutput (Output [failure.Message])
                | _ ->
                    gamestate
                    |> Output.setOutput (Output [sprintf "%s doesn't appear to have a switch for that." item.Name])
            | None ->
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    // let private tryUseItem uses name gamestate =
    //     match Uses.find uses gamestate.Environment with
    //     | Some (Unlock (exitId, desc))
    //     | Some (Unhide (exitId, desc)) ->
    //         let exit = Exit.find exitId gamestate.Environment
    //         gamestate
    //         |> Exit.openExit exit
    //         |> Output.setOutput (Output [desc; sprintf "%s opened with %s." exit.Description name;])
    //     | None ->
    //         gamestate
    //         |> Output.setOutput (Output [sprintf "Can't use %s here." name])

    // let useItem (itemName: string) : GamePart =
    //     fun gamestate ->
    //         // find item
    //         let itemOption =
    //             gamestate.Inventory
    //             |> List.tryFind (fun i -> ((Item.inventoryItemName i).ToLower()) = itemName.ToLower())

    //         let environmentItemOption =
    //             gamestate.Environment.EnvironmentItems
    //             |> List.tryFind (fun i -> ((Item.environmentItemDescription i).ToLower()) = itemName.ToLower())

    //         // use item
    //         match itemOption, environmentItemOption with
    //         | Some item', _ ->
    //             match item' with
    //             | InventoryItem item -> tryUseItem item.Uses item.Name gamestate
    //             | _ -> gamestate
                
    //         | None, Some item' ->
    //             match item' with
    //             | EnvironmentItem item -> tryUseItem item.Uses item.Name gamestate
    //             | _ ->
    //                 gamestate 
    //                 |> Output.setOutput (Output ["Can't use that here."])
    //         | None, _ ->
    //             gamestate
    //             |> Output.setOutput (Output [sprintf "What's a %s?" itemName])
            
    let save filename : GamePart =
        fun gamestate ->
            IO.saveGameState filename gamestate
            gamestate |> Output.setOutput (Output ["Game saved."])

    let undo : GamePart =
        fun gamestate ->
            gamestate
            |> Output.setOutput Rollback

module MainMenu =
    let startGame : GamePart =
        fun gamestate ->
            gamestate
            |> Scene.setScene OpenExplore
            |> Output.setOutput (Output [gamestate.Environment.Description])

    let loadGame : GamePart =
        fun _ ->
            IO.loadGameState "./SaveData/GameSave.json"

module InEncounter =
    let summarizeEncounter monsterPoints oldLevel : GamePart =
        fun gamestate ->
            let (Experience (points, level)) = gamestate.Player.Experience
            gamestate
            |> Output.appendOutputs [
                yield "The battle is over.";
                yield sprintf "You gained %i experience points. Total: %i" monsterPoints points;
                if level > oldLevel then yield sprintf "You are now level %i" level;                
            ]

    let finishEncounter experience : GamePart =
        fun gamestate ->
            let player' = gamestate.Player |> Player.addExperience experience
            let (Experience (_, oldLevel)) = gamestate.Player.Experience
            gamestate
            |> Player.setPlayer player'
            |> summarizeEncounter experience oldLevel
            |> Encounter.endEncounter
            |> Scene.setScene OpenExplore

    let playerAttack monster encounter =
        fun gamestate ->
            let playerDamage = (Damage 3) // TODO: use a weapon!!
            let playerPower = power gamestate.Player.Stats.Attack playerDamage
            if attackRoll monster.Stats.Defense playerPower Player.Rolls.d20Roll
            then
                // attack succeeds, update monster
                let health' = damage playerDamage monster.Health
                let monster' = monster |> Environment.Monster.setHealth health'
                let encounter' = monster' |> Encounter.updateMonster encounter
                let gamestate' =
                    gamestate
                    |> Encounter.updateEncounter encounter'
                    |> Scene.setScene (InEncounter encounter') // make sure to update the encounter in the scene
                    |> Output.appendOutputs [
                        sprintf "You hit %s with %A. %s" monster.Name playerDamage (healthDescription health')
                    ]
                (monster', encounter', gamestate')
            else
                // attack fails
                let gamestate' =
                    gamestate
                    |> Output.appendOutputs [
                        sprintf "You miss %s!" monster.Name
                    ]
                (monster, encounter, gamestate')

    let monsterAttack monster =
        fun gamestate ->
            if monster.Health |> isAlive then
                let monsterPower = power monster.Stats.Attack monster.Stats.Damage
                if attackRoll gamestate.Player.Stats.Defense monsterPower Player.Rolls.d20Roll
                then
                    // attack succeeds, update player
                    let health' = damage monster.Stats.Damage gamestate.Player.Health
                    let player' =
                        gamestate.Player
                        |> Player.setHealth health'
                    let gamestate' =
                        gamestate
                        |> setPlayer player'
                        |> Output.appendOutputs [
                            sprintf "%s hits you with %A. %s" monster.Name monster.Stats.Damage (healthDescription health')
                        ]
                    (player', gamestate')
                else
                    // attack fails
                    let gamestate' =
                        gamestate
                        |> Output.appendOutputs [
                            sprintf "%s misses you!" monster.Name
                        ]
                    (gamestate.Player, gamestate')
            else
                (gamestate.Player, gamestate)

    let attack : GamePart =
        fun gamestate ->
            match gamestate.GameScene with
            | InEncounter encounter ->
                let monsterOption = encounter |> Environment.Encounter.findAMonster
                match monsterOption with
                | Some monster ->
                    // TODO: figure out initiative, who goes first.
                    let (monster', encounter', gamestate') =
                        gamestate
                        |> Output.setOutput (Output ["You attack!"])
                        |> playerAttack monster encounter

                    let (player', gamestate'') =
                        gamestate'
                        |> monsterAttack monster'

                    if Environment.Encounter.checkForMonsters encounter' then
                        gamestate'' |> Player.checkGameOver
                    else
                        let monsterPoints = encounter.Monsters |> List.sumBy (fun {ExperiencePoints = points} -> points)
                        gamestate'' |> finishEncounter monsterPoints    
                | None ->
                    let monsterPoints = encounter.Monsters |> List.sumBy (fun {ExperiencePoints = points} -> points)
                    gamestate |> finishEncounter monsterPoints
            | _ ->
                gamestate
                |> Scene.setScene OpenExplore
    
    let run : GamePart =
        fun gamestate ->
            match gamestate.GameScene with
            | InEncounter _ ->
                gamestate
                |> Output.setOutput (Output ["You ran away."])
                |> Scene.setScene OpenExplore
            | _ ->
                gamestate
                |> Scene.setScene OpenExplore