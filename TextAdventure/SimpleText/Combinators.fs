module Combinators
open Domain
open GameState

type GamePart = GameState -> GameState

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

let noOp : GamePart =
    fun gamestate ->
        { gamestate with Output = Empty }

let move dir: GamePart =
    fun gamestate ->
        let exitOption = gamestate.Environment.Exits |> List.tryFind (fun p -> p.Direction = dir)
        match exitOption with
        | Some exit ->
            match exit.ExitState with
            | Open ->
                let nextEnvironment = 
                    gamestate.World.Map
                    |> Array.find (fun env -> env.Id = exit.Target)

                // calculate and add the travel time 
                let time = exit.Distance |> timespanFromDistance
                let world = {gamestate.World with Time = gamestate.World.Time + time }

                // log outputs
                let log = [nextEnvironment.Description]
                { gamestate with Environment = nextEnvironment; World = world; Output = Output log }
            | Locked ->
                { gamestate with Output = Output ["The exit is locked."]}
            | Hidden -> 
                { gamestate with Output = Output [sprintf "There are no exits to the %A." dir] }
        | None ->
            { gamestate with Output = Output [sprintf "There are no exits to the %A." dir] }

let look : GamePart =
    fun gamestate ->
        let exitHelper = sprintf "%s to the %A"
        let itemHelper = sprintf "%s %s"
        let exits = gamestate.Environment.Exits |> List.filter (fun e -> e.ExitState <> Hidden) |> List.map (fun p -> exitHelper p.Description p.Direction)
        let items = gamestate.Environment.InventoryItems |> List.map (inventoryItemProps >> (fun prop -> itemHelper prop.Name prop.Description))
        let log = [
            yield "There's a..."; yield! exits; 
            match items with [] -> () | _ -> yield ""; yield "You see a..."; yield! items ]
        { gamestate with Output = Output log }

let message s : GamePart =
    fun gamestate ->
        {gamestate with Output = Output [s]}

let take (itemName: string) : GamePart =
    fun gamestate ->
        // find item
        let itemOption = 
            gamestate.Environment.InventoryItems 
            |> List.tryFind (fun i -> ((inventoryItemName i).ToLower()) = itemName.ToLower())
        match itemOption with
        | Some item ->
            gamestate
            |> removeItemFromEnvironment item
            |> addItemToInventory item
            |> updateWorldEnvironment
            |> setOutput (Output [sprintf "You took %s." (inventoryItemName item)])
        | None ->    
            let output = [sprintf "Couldn't find %s." itemName]
            {gamestate with Output = Output output }

let drop (itemName: string) : GamePart =
    fun gamestate ->
        // find item
        let itemOption = 
            gamestate.Inventory
            |> List.tryFind (fun i -> ((inventoryItemName i).ToLower()) = itemName.ToLower())
        match itemOption with
        | Some item ->
            gamestate
            |> addItemToEnvironment item
            |> updateWorldEnvironment
            |> setOutput (Output [sprintf "You dropped %s." (inventoryItemName item)])
        | None ->    
            let output = [sprintf "Couldn't find %s." itemName]
            {gamestate with Output = Output output }

let tryUseItem uses name gamestate =
    match findUseInEnvironment uses gamestate.Environment with
    | Some (Unlock (exitId, desc))
    | Some (Unhide (exitId, desc)) ->
        let exit = findExit exitId gamestate.Environment
        gamestate
        |> updateEnvironmentExit { exit with ExitState = Open}
        |> updateWorldEnvironment
        |> setOutput (Output [desc; sprintf "%s opened with %s." exit.Description name;])
    | None ->
        gamestate
        |> setOutput (Output [sprintf "Can't use %s here." name])

let useItem (itemName: string) : GamePart =
    fun gamestate ->
        // find item
        let itemOption =
            gamestate.Inventory
            |> List.tryFind (fun i -> ((inventoryItemName i).ToLower()) = itemName.ToLower())

        let environmentItemOption =
            gamestate.Environment.EnvironmentItems
            |> List.tryFind (fun i -> ((environmentItemDescription i).ToLower()) = itemName.ToLower())

        // use item
        match itemOption, environmentItemOption with
        | Some item', _ ->
            match item' with
            | InventoryItem item -> tryUseItem item.Uses item.Name gamestate
        | None, Some item' ->
            match item' with
            | EnvironmentItem item -> tryUseItem item.Uses item.Name gamestate
        | None, _ ->
            gamestate
            |> setOutput (Output [sprintf "What's a %s?" itemName])
        
let save filename : GamePart =
    fun gamestate ->
        saveGameState filename gamestate
        gamestate |> setOutput (Output ["Game saved."])