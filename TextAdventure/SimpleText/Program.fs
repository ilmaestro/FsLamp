// Run the game!
[<EntryPoint>]
let main _ =
    Game.RunInConsole Parser.simpleParser Dispatcher.dispatch Game.defaultGamestate
    0