module Actions
open Domain
open GameState
open Environment
open Player
open System

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
                    |> updateWorldTravelTime exit.Distance
                    |> setEnvironment nextEnvironment
                    |> setOutput (Output [nextEnvironment.Description])
                    |> Encounter.checkEncounter
                | Locked ->
                    gamestate |> setOutput (Output ["The exit is locked."])
                | Hidden -> 
                    gamestate |> setOutput (Output [sprintf "There are no exits to the %A." dir])
            | None ->
                gamestate |> setOutput (Output [sprintf "There are no exits to the %A." dir])

    let look : GamePart =
        fun gamestate ->
            let exitHelper = sprintf "\tA %s to the %A"
            let itemHelper = sprintf "\tA %s %s"
            let exits = gamestate.Environment.Exits |> List.filter (fun e -> e.ExitState <> Hidden) |> List.map (fun p -> exitHelper p.Description p.Direction)
            let items = gamestate.Environment.InventoryItems |> List.map (Item.inventoryItemProps >> (fun (name, description) -> itemHelper name description))
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
                |> List.tryFind (fun i -> ((Item.inventoryItemName i).ToLower()) = itemName.ToLower())
            match itemOption with
            | Some item ->
                gamestate
                |> Item.removeItemFromEnvironment item
                |> Item.addItem item
                |> updateWorldEnvironment
                |> setOutput (Output [sprintf "You took %s." (Item.inventoryItemName item)])
            | None ->    
                let output = [sprintf "Couldn't find %s." itemName]
                {gamestate with Output = Output output }

    let drop (itemName: string) : GamePart =
        fun gamestate ->
            // find item
            let itemOption = 
                gamestate.Inventory
                |> List.tryFind (fun i -> ((Item.inventoryItemName i).ToLower()) = itemName.ToLower())
            match itemOption with
            | Some item ->
                gamestate
                |> Item.addItemToEnvironment item
                |> updateWorldEnvironment
                |> setOutput (Output [sprintf "You dropped %s." (Item.inventoryItemName item)])
            | None ->    
                let output = [sprintf "Couldn't find %s." itemName]
                {gamestate with Output = Output output }

    let private tryUseItem uses name gamestate =
        match Uses.find uses gamestate.Environment with
        | Some (Unlock (exitId, desc))
        | Some (Unhide (exitId, desc)) ->
            let exit = Exit.find exitId gamestate.Environment
            gamestate
            |> Exit.openExit exit
            |> setOutput (Output [desc; sprintf "%s opened with %s." exit.Description name;])
        | None ->
            gamestate
            |> setOutput (Output [sprintf "Can't use %s here." name])

    let useItem (itemName: string) : GamePart =
        fun gamestate ->
            // find item
            let itemOption =
                gamestate.Inventory
                |> List.tryFind (fun i -> ((Item.inventoryItemName i).ToLower()) = itemName.ToLower())

            let environmentItemOption =
                gamestate.Environment.EnvironmentItems
                |> List.tryFind (fun i -> ((Item.environmentItemDescription i).ToLower()) = itemName.ToLower())

            // use item
            match itemOption, environmentItemOption with
            | Some item', _ ->
                match item' with
                | InventoryItem item -> tryUseItem item.Uses item.Name gamestate
                | _ -> gamestate
                
            | None, Some item' ->
                match item' with
                | EnvironmentItem item -> tryUseItem item.Uses item.Name gamestate
                | _ ->
                    gamestate 
                    |> setOutput (Output ["Can't use that here."])
            | None, _ ->
                gamestate
                |> setOutput (Output [sprintf "What's a %s?" itemName])
            
    let save filename : GamePart =
        fun gamestate ->
            saveGameState filename gamestate
            gamestate |> setOutput (Output ["Game saved."])

    let undo : GamePart =
        fun gamestate ->
            gamestate
            |> setOutput Rollback

module MainMenu =
    let startGame : GamePart =
        fun gamestate ->
            gamestate
            |> setScene OpenExplore
            |> setOutput (Output [gamestate.Environment.Description])

    let loadGame : GamePart =
        fun _ ->
            loadGameState "./SaveData/GameSave.json"

module InEncounter =
    let summarizeEncounter monsterPoints oldLevel : GamePart =
        fun gamestate ->
            let (Experience (points, level)) = gamestate.Player.Experience
            gamestate
            |> appendOutputs [
                yield "The battle is over.";
                yield sprintf "You gained %i experience points. Total: %i" monsterPoints points;
                if level > oldLevel then yield sprintf "You are now level %i" level;                
            ]

    let finishEncounter experience : GamePart =
        fun gamestate ->
            let player' = gamestate.Player |> Player.addExperience experience
            let (Experience (_, oldLevel)) = gamestate.Player.Experience
            gamestate
            |> setPlayer player'
            |> summarizeEncounter experience oldLevel
            |> Encounter.endEncounter
            |> setScene OpenExplore

    let playerAttack monster encounter =
        fun gamestate ->
            let playerDamage = (Damage 3) // TODO: use a weapon!!
            let playerPower = power gamestate.Player.Attack playerDamage
            if attackRoll monster.Defense playerPower Player.Rolls.d20Roll
            then
                // attack succeeds, update monster
                let health' = damage playerDamage monster.Health
                let monster' = monster |> Environment.Monster.setHealth health'
                let encounter' = monster' |> Encounter.updateMonster encounter
                let gamestate' =
                    gamestate
                    |> Encounter.updateEncounter encounter'
                    |> setScene (InEncounter encounter') // make sure to update the encounter in the scene
                    |> appendOutputs [
                        sprintf "You hit %s with %A. %s" monster.Name playerDamage (healthDescription health')
                    ]
                (monster', encounter', gamestate')
            else
                // attack fails
                let gamestate' =
                    gamestate
                    |> appendOutputs [
                        sprintf "You miss %s!" monster.Name
                    ]
                (monster, encounter, gamestate')

    let monsterAttack monster =
        fun gamestate ->
            if monster.Health |> isAlive then
                let monsterPower = power monster.Attack monster.Damage
                if attackRoll gamestate.Player.Defense monsterPower Player.Rolls.d20Roll
                then
                    // attack succeeds, update player
                    let health' = damage monster.Damage gamestate.Player.Health
                    let player' =
                        gamestate.Player
                        |> Player.setHealth health'
                    let gamestate' =
                        gamestate
                        |> setPlayer player'
                        |> appendOutputs [
                            sprintf "%s hits you with %A. %s" monster.Name monster.Damage (healthDescription health')
                        ]
                    (player', gamestate')
                else
                    // attack fails
                    let gamestate' =
                        gamestate
                        |> appendOutputs [
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
                        |> setOutput (Output ["You attack!"])
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
                |> setScene OpenExplore
    
    let run : GamePart =
        fun gamestate ->
            match gamestate.GameScene with
            | InEncounter _ ->
                gamestate
                |> setOutput (Output ["You ran away."])
                |> setScene OpenExplore
            | _ ->
                gamestate
                |> setScene OpenExplore