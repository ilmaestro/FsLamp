open Domain
open Actions
open Game
open GameMap
open GameState
open LUISApi.Model
open Parser
open Microsoft.Extensions.Configuration

let configuration = 
    ((new ConfigurationBuilder())
            .AddJsonFile("appsettings.json")
            .Build()) :> IConfiguration

let getLuisSettings (config: IConfiguration) =
    { Region = config.["LUIS:region"]; AppId = config.["LUIS:appId"]; AppKey = config.["LUIS:appKey"] }

[<EntryPoint>]
let main _ =
    let luisSettings = getLuisSettings configuration
    
    let actionResolver gameScene = 
        let parser = 
            match gameScene with
            | MainMenu -> mainMenuParser
            | OpenExplore -> (Parser.luisParser luisSettings) //exploreParser
            | InEncounter _ -> encounterParser
        Console.getCommand parser |> Dispatcher.dispatch

    RunGame actionResolver (defaultGamestate (defaultMap()))
    0