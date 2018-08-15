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
            let playerStats = gamestate.Player |> playerStatus
            let inventory = gamestate.Inventory |> Items.inventoryStatus
            let outputs = [
                playerStats;
                inventory
            ]
            { gamestate with Output = Output outputs }
    let noOp : GamePart =
        fun gamestate ->
            { gamestate with Output = DoNothing }
    let message s : GamePart =
        fun gamestate ->
            {gamestate with Output = Output [s]}

    let ifLightSource f g : GamePart =
        fun gamestate ->
            // either you have an item that's providing a lightsource or the current environment has its own
            match gamestate.Environment.LightSource, (findItemByUse Items.ProvidesLight gamestate.Inventory)  with
            | Some _, _ -> f gamestate
            | _, Some item when item |> itemIsSwitchedOn -> f gamestate
            | _ -> g gamestate

    let bindId a b = a b

module Explore =
    
    let useDescribe itemName itemUse : GamePart =
        fun gamestate ->
            let itemOption = tryFindItemFromGame itemName gamestate
            let description = 
                maybe {
                    let! item' = itemOption
                    let! (desc, _) = item' |> tryFindItemUse itemUse
                    return desc
                }
            match itemOption, description with
            | Some _, Some (Description desc) ->
                gamestate
                |> Output.setOutput (Output [desc])
            | Some _, None ->
                gamestate 
                |> Output.setOutput (Output [sprintf "%s doesn't appear to have that function." itemName])
            | None, _ ->
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    let useGeneric itemName uses handleItemUpdate handleGamestateUpdate doItemUpdate doGameStateUpdate : GamePart =
        fun gamestate ->
            let itemOption = tryFindItemFromGame itemName gamestate
            let useBehavior =
                maybe {
                    let! item' = itemOption
                    let! (desc, itemUse) = item' |> tryFindOneOf uses
                    let! behavior = (desc, itemUse) |> tryFindBehaviorFromUse
                    return (desc, itemUse, behavior)
                }
            match itemOption, useBehavior with
            | Some item, Some (desc, itemUse, UpdateItem update) ->
                match doItemUpdate update (itemUse, item) with
                | Ok item' ->
                    gamestate |> handleItemUpdate (item', itemUse, desc)
                | Error (failure: UpdateItemFailure) ->
                    gamestate
                    |> Output.setOutput (Output [failure.Message])
            | Some item, Some (desc, itemUse, UpdateGameState update) ->
                match doGameStateUpdate update (itemUse, item, gamestate) with
                | Ok gamestate' ->
                    gamestate' |> handleGamestateUpdate (item, itemUse, desc)
                | Error failure ->
                    failure.GameState
                    |> Output.setOutput (Output [failure.Message])
            | Some _, None ->
                gamestate |> Output.setOutput (Output [sprintf "%s doesn't appear to have that function." itemName])
            | None, Some _
            | None, None ->
                gamestate |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

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


    let help : GamePart = Output.setOutput (Output [Utility.readTextAsset "Help.md"])

    let exitGame : GamePart = Output.setOutput ExitGame

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
                        (Output.setOutput (Output [(nextEnvironment.Describe())]))
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
            let exitHelper = sprintf "- A %s (%A)"
            let itemHelper = sprintf "- A %s %s"
            let exits = gamestate.Environment.Exits |> List.filter (fun e -> e.ExitState <> Hidden) |> List.map (fun p -> exitHelper p.Description p.Direction)
            let items = gamestate.Environment.InventoryItems |> List.map (fun {Name = name; Description = description } -> itemHelper name description)
            let log = [
                yield "You look around and see..."
                yield "*Exits*";
                yield! exits; 
                match items with
                | [] -> () 
                | _ ->
                    yield "*Interesting shtuff*"; 
                    yield! items 
                ]

            gamestate
            |> ifLightSource
                (Output.setOutput (Output log))
                (Output.setOutput (Output ["It's too dark to look around."]))

    let lookIn itemName : GamePart =
        fun gamestate ->
            match tryFindItemFromGame itemName gamestate with
            | Some item ->
                match item.Contains with
                | Some contents when contents.Length > 0 ->
                    let outputs = contents |> List.map (fun i -> sprintf "- A %s" i.Name)
                    gamestate |> Output.setOutput (Output ("*You see*" :: outputs))
                | _ ->
                    gamestate |> Output.setOutput (Output [sprintf "There's nothing inside %s." itemName])
            | None ->
                gamestate |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    let take (itemName: string) : GamePart =
        useGeneric itemName [ItemUse.Defaults.CanTake]
            (fun _ gamestate -> gamestate)
            (fun (_, _, Description desc) gamestate -> gamestate |> Output.prependOutputs [desc])
            bindId
            bindId
    
    let takeFrom targetName itemName : GamePart =
        let takeOutUse = Items.TakeOut (Some targetName)
        useGeneric itemName [ItemUse.Defaults.TakeOut]
            (fun (_, _, _) gamestate -> gamestate)
            (fun (_, _, Description desc) gamestate -> gamestate |> Output.appendOutputs [desc])
            (fun update (_, item) -> update (takeOutUse, item))
            (fun update (_, item, gamestate) -> update (takeOutUse, item, gamestate))

    let drop (itemName: string) : GamePart =
        fun gamestate ->
            // find item
            let itemOption = 
                gamestate.Inventory
                |> List.tryFind (fun i -> (i.Name.ToLower()) = itemName.ToLower())
            match itemOption with
            | Some item ->
                let inventory' = gamestate.Inventory |> List.except (seq { yield item })
                gamestate
                |> Environment.addItemToEnvironment item
                |> World.updateWorldEnvironment
                |> Inventory.setInventory inventory'
                |> Output.setOutput (Output [sprintf "You dropped %s." item.Name])
            | None ->    
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])

    let switch (itemName: string) switchState : GamePart =
        useGeneric itemName [ItemUse.Defaults.TurnOnOff]
            (fun (item, _, Description desc) gamestate ->
                let otherOutputs =
                    if (item |> ItemUse.itemHasUse Items.ProvidesLight) && gamestate.Environment.LightSource.IsNone && switchState = (Items.SwitchOn)
                    then
                        // this thing is providing light, describe the environment.
                        [gamestate.Environment.Describe()]
                    else []
                // TODO: if item isn't in Inventory, update it in environment
                gamestate 
                |> Inventory.updateItem item
                |> Output.setOutput (Output ([desc; sprintf "%s turned %s" item.Name (switchState.ToString());] @ otherOutputs)))
            (fun (_, _, Description desc) gamestate ->
                gamestate |> Output.setOutput (Output [desc]))
            (fun update (_, item) -> update (Items.TurnOnOff switchState, item))
            (fun update (_, item, gamestate) -> update (Items.TurnOnOff switchState, item, gamestate))

    let readItem (itemName: string) : GamePart =
        useDescribe itemName Items.Readable

    let useItem (itemName: string) : GamePart =
        useGeneric itemName ItemUse.Defaults.Useable
            (fun (item, itemUse, Description desc) gamestate ->
                // TODO: if item isn't in Inventory, update it in environment
                gamestate 
                |> Inventory.updateItem item
                |> Output.setOutput (Output [sprintf "Used %s." item.Name; desc]))
            (fun (item, itemUse, Description desc) gamestate ->
                gamestate |> Output.setOutput (Output [desc]))
            bindId
            bindId

    let useItemOn (targetName: string) (itemName: string) : GamePart =
        fun gamestate ->
            if targetName = "door" then 
                gamestate |> useItem itemName 
            else gamestate

    let put sourceName targetName : GamePart =
        fun gamestate ->
            match tryFindItemFromGame sourceName gamestate with
            | Some sourceItem ->
                let putInUse = Items.PutIn (Some sourceItem)
                gamestate
                |> useGeneric targetName [ItemUse.Defaults.PutIn]
                    (fun (_, _, _) gamestate -> gamestate)
                    (fun (_, _, Description desc) gamestate -> gamestate |> Output.setOutput (Output [desc]))
                    (fun update (_, item) -> update (putInUse, item))
                    (fun update (_, item, gamestate) -> update (putInUse, item, gamestate))
            | None ->
                gamestate
                |> Output.setOutput (Output [sprintf "Couldn't find %s." sourceName])

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
            |> Output.setOutput (Output [(gamestate.Environment.Describe())])

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