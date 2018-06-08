open Game

[<EntryPoint>]
let main _ =
    // printfn "%s" title
    // printfn "Type GO to start the game, or LOAD to start from saved game."
    // let command = System.Console.ReadLine()
    // let gamestate =
    //   if command = "LOAD" then
    //     GameState.loadGameState "./SaveData/GameSave.json"
    //   else defaultGamestate defaultMap

    // gamestate
    // |> RunInConsole 
    //     Parser.simpleParser
    //     Parser.simpleEncounterParser
    //     Dispatcher.dispatch
    //     Dispatcher.dispatchEncounter

    RunGame Dispatcher.dispatch (defaultGamestate defaultMap)
    0