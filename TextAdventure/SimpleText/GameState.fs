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