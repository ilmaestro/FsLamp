module GameState
open Domain
open Newtonsoft.Json

type GameScene =
| MainMenu
| OpenExplore
| InEncounter of EncounterProperties

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Inventory: InventoryItem list
    Environment: Environment
    World: World
    GameScene: GameScene
    LastCommand: Command
    Output: Output
}

type GameObject<'a> = GameObject of 'a * (GameState -> GameState)
type GameObject'<'a> = {
    Properties: 'a
    Update: ('a -> 'a)
    GetOutput: ('a -> GameState -> Output)
}

type GameHistory = GameState list


let setScene scene gamestate =
    { gamestate with GameScene = scene }


let setExperience experience gamestate =
    { gamestate with Experience = experience }

// World
let updateWorldEnvironment gamestate =
    // replace environment in the map
    let map =
        gamestate.World.Map
        |> Array.map (fun env ->
            if env.Id = gamestate.Environment.Id then gamestate.Environment else env)
    let world = {gamestate.World with Map = map }
    { gamestate with World = world }

let updateWorldTravelTime distance gamestate =
    let time = distance |> timespanFromDistance
    let world = {gamestate.World with Time = gamestate.World.Time + time }
    { gamestate with World = world }

let setEnvironment environment gamestate =
    { gamestate with Environment = environment }

// Output
let setOutput output gamestate =
    { gamestate with Output = output }

let addOutput s gamestate =
    match gamestate.Output with
    | Output output ->
        { gamestate with Output = Output (output @ [s]) }
    | _ -> 
        gamestate

let saveGameState filename gamestate =
    let json = JsonConvert.SerializeObject(gamestate)
    System.IO.File.WriteAllText(filename, json)

let loadGameState filename =
    let json = System.IO.File.ReadAllText(filename)
    JsonConvert.DeserializeObject<GameState>(json)