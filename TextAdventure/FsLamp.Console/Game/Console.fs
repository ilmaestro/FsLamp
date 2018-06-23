module Console
open System
open Domain
open GameState
open Parser

let private writeOutput (output: string []) =
    //output |> List.iter (printfn "%s")
    let s = String.Join ("\n", output)
    ConsoleService.Markdown.renderSomething s

let private writeOutputLog = function
    | Output log ->
        log |> List.toArray |> writeOutput
    | _ -> ()

let update (gs: GameState) =
    gs.Output |> writeOutputLog

let getCommand (parseInput: CommandParser) =
    Console.Write("\n> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand
