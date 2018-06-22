module Console
open System
open Domain
open GameState
open Parser

let private writeOutput output =
    output |> List.iter (printfn "%s")

let private writeOutputLog = function
    | Output log ->
        log |> writeOutput
    | _ -> ()

let update gs =
    gs.Output |> writeOutputLog

let getCommand (parseInput: CommandParser) =
    Console.Write("\n> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand
