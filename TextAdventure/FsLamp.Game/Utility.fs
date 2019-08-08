module FsLamp.Game.Utility

open Newtonsoft.Json
open FsLamp.Core.GameState

let readFile folder file =
    System.IO.File.ReadAllText(System.IO.Path.Combine(folder, file))

let readAsset = readFile "../Assets"
let readTextAsset = readFile "../Assets/Text"

let saveGameState filename gamestate =
    let json = JsonConvert.SerializeObject(gamestate)
    System.IO.File.WriteAllText(filename, json)

let loadGameState filename =
    let json = System.IO.File.ReadAllText(filename)
    JsonConvert.DeserializeObject<GameState>(json)