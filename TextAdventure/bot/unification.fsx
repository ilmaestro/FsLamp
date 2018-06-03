module Unification
#if INTERACTIVE
#r "../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "bot.fsx"
#load "phrases.fsx"
#endif

open Eliza
open Phrases

let rec unification pattern input acc =
    match pattern, input with
    // TODO: complete patterns...
    | [],[] ->
        acc |> List.rev |> Some
    | _ -> None