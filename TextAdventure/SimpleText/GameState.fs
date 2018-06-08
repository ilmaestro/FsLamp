module GameState
open Domain
open Newtonsoft.Json

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Inventory: InventoryItem list
    Environment: Environment
    World: World
    LastCommand: Command
    Output: Output
}

type GameHistory = GameState list


// Inventory
let addItemToInventory item gamestate =
    { gamestate with Inventory = item :: gamestate.Inventory }

// Environment
let removeItemFromEnvironment item gamestate =
    let environment = {gamestate.Environment with InventoryItems = gamestate.Environment.InventoryItems |> List.filter (fun i -> i <> item) }
    { gamestate with Environment = environment}

let addItemToEnvironment item gamestate =
    let environment = {gamestate.Environment with InventoryItems = item :: gamestate.Environment.InventoryItems }
    { gamestate with Environment = environment}

// Exits
let updateEnvironmentExit exit gamestate =
    let exits = gamestate.Environment.Exits |> List.map (fun e -> if e.Id = exit.Id then exit else e)
    let environment = {gamestate.Environment with Exits = exits }
    { gamestate with Environment = environment}   

// World
let updateWorldEnvironment gamestate =
    // replace environment in the map
    let map =
        gamestate.World.Map
        |> Array.map (fun env ->
            if env.Id = gamestate.Environment.Id then gamestate.Environment else env)
    let world = {gamestate.World with Map = map }
    { gamestate with World = world }

// Output
let setOutput output gamestate =
    { gamestate with Output = output }

let saveGameState filename gamestate =
    let json = JsonConvert.SerializeObject(gamestate)
    System.IO.File.WriteAllText(filename, json)

let loadGameState filename =
    let json = System.IO.File.ReadAllText(filename)
    JsonConvert.DeserializeObject<GameState>(json)

// compositions
let openExitWithItem openExit itemName gamestate =
    gamestate
    |> updateEnvironmentExit openExit
    |> updateWorldEnvironment
    |> setOutput (Output [sprintf "%s opened with %s" openExit.Description itemName])

let findUseInEnvironment uses environment =
    uses
    |> List.map(fun u -> 
        match u with
        | Unlock (exitId, _)
        | Unhide (exitId, _) ->
            environment.Exits 
            |> List.tryFind (fun e -> e.Id = exitId && e.ExitState <> Open)
            |> Option.map (fun _ -> u)
    )
    |> List.choose id
    |> List.tryHead