open Game
open GameMap

[<EntryPoint>]
let main _ =
    RunGame Dispatcher.dispatch (defaultGamestate (defaultMap()))
    0