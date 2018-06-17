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

    // let tryFindItemWithUseFromInventory (itemName: string) itemUse gamestate =
    //     gamestate.Inventory
    //     |> List.tryFind (fun i ->
    //         (i.Name.ToLower()) = itemName.ToLower())
    //     |> Option.bind (tryFindItemUse itemUse)

    let findItemByUse itemUse items =
        items |> List.tryFind (tryFindItemUse itemUse >> Option.isSome)

    // let anyItemWithUse itemUse items =
    //     items |> List.exists (tryFindItemUse itemUse >> Option.isSome)

    let itemIsSwitchedOn (item: Items.InventoryItem) =
        match item.SwitchState with
        | Some Items.SwitchOn -> true
        | _ -> false
    let tryFindByName (name: string) (list: Items.InventoryItem list) =
        list |> List.tryFind (fun i -> i.Name.ToLower() = name.ToLower())

    let tryFindUpdate f itemUse item =
        item
        |> tryFindItemUse itemUse
        |> Option.bind f
        |> Option.map (fun update -> (item, itemUse, update))

    let tryFindItemFromGame name gamestate =
        [gamestate.Inventory; gamestate.Environment.InventoryItems]
        |> List.map (tryFindByName name)
        |> List.choose id
        |> List.tryHead

    let tryFindItemBehavior itemUse item =

        let itemUpdate = 
            tryFindUpdate findItemUseBehavior itemUse item
            |> Option.map (fun (item, itemUse, update) -> (item, itemUse, UpdateItem update))
        let gameUpdate =
            tryFindUpdate findGameStateBehavior itemUse item
            |> Option.map (fun (item, itemUse, update) -> (item, itemUse, UpdateGameState update))
        [itemUpdate; gameUpdate]
        |> List.choose id
        |> List.tryHead

    let tryFindBehaviorFromUse itemUse =
        let itemUpdate = findItemUseBehavior itemUse |> Option.map UpdateItem
        let gameUpdate = findGameStateBehavior itemUse |> Option.map UpdateGameState
        [itemUpdate; gameUpdate]
        |> List.choose id
        |> List.tryHead

    let tryFindOneOf uses item =
        uses
        |> List.map (fun itemuse -> item |> tryFindItemUse itemuse)
        |> List.choose id
        |> List.tryHead
        
    let ifLightSource f g : GamePart =
        fun gamestate ->
            // either you have an item that's providing a lightsource or the current environment has its own
            match gamestate.Environment.LightSource, (findItemByUse Items.ProvidesLight gamestate.Inventory)  with
            | Some _, _ -> f gamestate
            | _, Some item when item |> itemIsSwitchedOn -> f gamestate
            | _ -> g gamestate

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
                    |> ifLightSource
                        (Output.setOutput (Output [nextEnvironment.Description]))
                        (Output.setOutput (Output ["It's too dark to see."]))
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

            gamestate
            |> ifLightSource
                (Output.setOutput (Output log))
                (Output.setOutput (Output ["It's too dark to look around."]))

    let take (itemName: string) : GamePart =
        fun gamestate ->
            let tryTakeUpdate = 
                gamestate.Environment.InventoryItems 
                |> tryFindByName itemName
                |> Option.bind (tryFindUpdate findGameStateBehavior (ItemUse.Defaults.CanTake))
                |> Option.map (fun (item, itemUse, update) -> update (itemUse, item, gamestate))

            match tryTakeUpdate with
            | Some (Ok gs) -> gs
            | Some (Error failure) -> failure.GameState
            | None -> 
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

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
            let item = tryFindItemFromGame itemName gamestate
            let behavior = item |> Option.bind (tryFindItemBehavior (ItemUse.Defaults.TurnOnOff))
            
            match item, behavior with
            | _, Some (item, _, UpdateItem update) ->
                match update (Items.TurnOnOff switchState, item) with
                | Ok item' ->
                    // TODO: if item isn't in Inventory, update it in environment
                    gamestate 
                    |> Inventory.updateItem item'
                    |> Output.setOutput (Output [sprintf "%s turned %A" item.Name switchState])
                | Error failure ->
                    gamestate
                    |> Output.setOutput (Output [failure.Message])
            | _, Some (item, _, UpdateGameState update) ->
                match update (Items.TurnOnOff switchState, item, gamestate) with
                | Ok gamestate' ->
                    gamestate'
                | Error failure ->
                    failure.GameState
            | Some _, None ->
                gamestate |> Output.setOutput (Output [sprintf "%s doesn't appear to have that function." itemName])
            | None, None ->
                gamestate |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])


    let useItem (itemName: string) : GamePart =
        fun gamestate ->
            let itemOption = tryFindItemFromGame itemName gamestate
            let useBehavior =
                maybe {
                    let! item' = itemOption
                    let! (desc, itemUse) = item' |> tryFindOneOf ItemUse.Defaults.Useable
                    let! behavior = (desc, itemUse) |> tryFindBehaviorFromUse
                    return (desc, itemUse, behavior)
                }
            
            match itemOption, useBehavior with
            | Some item, Some (Description desc, itemUse, UpdateItem update) ->
                match update (itemUse, item) with
                | Ok item' ->
                    // TODO: if item isn't in Inventory, update it in environment
                    gamestate 
                    |> Inventory.updateItem item'
                    |> Output.setOutput (Output [sprintf "Used %s." item.Name; desc])
                | Error failure ->
                    gamestate
                    |> Output.setOutput (Output [failure.Message])
            | Some item, Some (Description desc, itemUse, UpdateGameState update) ->
                match update (itemUse, item, gamestate) with
                | Ok gamestate' ->
                    gamestate' |> Output.setOutput (Output [desc])
                | Error failure ->
                    failure.GameState
            | Some _, None ->
                gamestate |> Output.setOutput (Output [sprintf "%s doesn't appear to have that function." itemName])
            | None, Some _
            | None, None ->
                gamestate |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    let useItemOn (targetName: string) (itemName: string) : GamePart =
        fun gamestate ->
            if targetName = "door" then 
                gamestate |> useItem itemName 
            else gamestate

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