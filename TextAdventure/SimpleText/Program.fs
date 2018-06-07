open Game

let title = """
,d88~~\ ,e,                         888                 ~~~888~~~                      d8   
8888     "  888-~88e-~88e 888-~88e  888  e88~~8e           888     e88~~8e  Y88b  /  _d88__ 
`Y88b   888 888  888  888 888  888b 888 d888  88b          888    d888  88b  Y88b/    888   
 `Y88b, 888 888  888  888 888  8888 888 8888__888          888    8888__888   Y88b    888   
   8888 888 888  888  888 888  888P 888 Y888    ,          888    Y888    ,   /Y88b   888   
\__88P' 888 888  888  888 888-_88"  888  "88___/           888     "88___/   /  Y88b  "88_/ 
                          888                                                               
"""

[<EntryPoint>]
let main _ =
    printfn "%s" title
    printfn "Type GO to start the game, or LOAD to start from saved game."
    let command = System.Console.ReadLine()
    let gamestate =
      if command = "LOAD" then
        GameState.loadGameState "./SaveData/GameSave.json"
      else defaultGamestate defaultMap

    RunInConsole Parser.simpleParser Dispatcher.dispatch gamestate
    0