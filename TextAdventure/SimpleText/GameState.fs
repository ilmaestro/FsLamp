module GameState
open Domain
open Newtonsoft.Json

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Inventory: Item list
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
    let environment = {gamestate.Environment with Items = gamestate.Environment.Items |> List.filter (fun i -> i <> item) }
    { gamestate with Environment = environment}

let addItemToEnvironment item gamestate =
    let environment = {gamestate.Environment with Items = item :: gamestate.Environment.Items }
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