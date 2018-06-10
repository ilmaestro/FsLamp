module Actions
open Domain
open GameState
open Environment

type GamePart = GameState -> GameState

[<AutoOpen>]
module Common =
    let status : GamePart =
        fun gamestate ->
            let outputs = [
                sprintf "%A" gamestate.Player;
                sprintf "%A" gamestate.Health;
                sprintf "%A" gamestate.Experience;
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
            let exitHelper = sprintf "%s to the %A"
            let itemHelper = sprintf "%s %s"
            let exits = gamestate.Environment.Exits |> List.filter (fun e -> e.ExitState <> Hidden) |> List.map (fun p -> exitHelper p.Description p.Direction)
            let items = gamestate.Environment.InventoryItems |> List.map (Item.inventoryItemProps >> (fun prop -> itemHelper prop.Name prop.Description))
            let log = [
                yield "There's a..."; yield! exits; 
                match items with [] -> () | _ -> yield ""; yield "You see a..."; yield! items ]
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
            let (points, level) = Player.getExperience gamestate

            let outputs = [
                yield "You attack and win!";
                yield sprintf "You gained %i experience points. Total: %i" monsterPoints points;
                if level > oldLevel then yield sprintf "You are now level %i" level;                
            ]
            gamestate
            |> setOutput (Output outputs)

    let attack : GamePart =
        fun gamestate ->
            match gamestate.GameScene with
            | InEncounter encounter ->
                let monsterPoints = encounter.Monsters |> List.sumBy (fun { ExperiencePoints = points} -> points)
                let (_, level) = Player.getExperience gamestate
                gamestate
                |> Player.addExperience monsterPoints
                |> summarizeEncounter monsterPoints level
                |> Encounter.endEncounter
                |> setScene OpenExplore
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