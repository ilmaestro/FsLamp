module Phrases
#if INTERACTIVE
#r "../../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "bot.fsx"
#endif

open FSharp.Data
open Eliza

type PsychoPhrases = JsonProvider<"[{\"pattern\":\"Something\",\"answers\":[\"Something else.\"]}]">
type Reflections = JsonProvider<"[{\"input\":\"blah\", \"output\": \"blah\"}]">

let phrases =
    PsychoPhrases.Load("../data/phrases.json")
    |> Array.toList
    |> List.map (fun row -> 
        { InputPattern = row.Pattern |> makeSentence; 
            AnswerPatterns = row.Answers |> Array.map makePatterns } 
    )

let reflections =
    Reflections.Load("../data/reflections.json")
    |> Array.map (fun refl -> (refl.Input, refl.Output))
    |> dict

let reflect (text: string list) =
    text
    |> List.map (fun word -> 
        if reflections.ContainsKey word 
        then reflections.[word] 
        else word)
