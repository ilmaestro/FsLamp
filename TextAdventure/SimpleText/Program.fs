// Run the game!
open Game

[<EntryPoint>]
let main _ =
    let gamestate = defaultGamestate defaultMap
    RunInConsole Parser.simpleParser Dispatcher.dispatch gamestate
    0