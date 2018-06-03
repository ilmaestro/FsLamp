open Domain
open Game

[<EntryPoint>]
let main _ =
    let stateHandler : GamePart = setEnvironment >> setOutput
    RunGame simpleParser setInput stateHandler defaultGamestate
    0