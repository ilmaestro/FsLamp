module Console
open System
open FsLamp.Core.Domain
open FsLamp.Core.GameState
open FsLamp.Core.Primitives

let private writeOutputLog = function
    | Output log ->
        let output = log |> List.toArray
        String.Join ("\n", output)
    | _ -> ""

let update (gs: GameState) =
    let (Health (cur,max)) = gs.Player.Health
    let health = sprintf "%i/%i" cur max
    let (Experience (exp, _)) = gs.Player.Experience
    let time = gs.World.Time.ToString()
    ConsoleService.PageView.drawScreen (gs.Output |> writeOutputLog) gs.Environment.Name health exp time

let getCommand (parseInput: CommandParser) =
    Console.Write("\n> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand

type ConsoleRenderer() =
    interface FsLamp.Core.IRenderer with
        member x.RenderGameState(gs) =
            let (Health (cur,max)) = gs.Player.Health
            let health = sprintf "%i/%i" cur max
            let (Experience (exp, _)) = gs.Player.Experience
            let time = gs.World.Time.ToString()
            ConsoleService.PageView.drawScreen (gs.Output |> writeOutputLog) gs.Environment.Name health exp time

        member x.RenderMarkdown(input) =
            ConsoleService.Markdown.renderSomething input
        
        member x.Clear() =
            ConsoleService.clearScreen()

        member x.Init() = 
            ConsoleService.useAlternateScreenBuffer ()
            ConsoleService.clearScreen()
           
        member x.Close() =
            ConsoleService.useMainScreenBuffer()