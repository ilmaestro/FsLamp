module Eliza

type Pattern =
| Word of string
| Wildcard

type Sentence = {
    Contents: Pattern list
    IsQuestion: bool
}

type Answer = Pattern list

type Phrases = {
    InputPattern: Sentence
    AnswerPatterns: Answer []
}
let cleanText (input: string) =
    let trim (s: string) = s.Trim()
    [("?", " "); (".", " "); ("!", " "); (",", " "); ("  ", " ")]
    |> List.fold (fun (acc: string) (key,value) -> 
        acc.Replace(key,value)) (input.ToLower())
    |> trim

let parseText (input: string) : Pattern list =
    input.Split([| ' ' |])
    |> Array.toList
    |> List.map (fun word -> if word = "*" then Wildcard else Word word)

let isQuestion (input: string) =
    input.EndsWith("?")

let makePatterns input =
    input |> cleanText |> parseText
  
let makeSentence input =
    { Contents = input |> makePatterns;
        IsQuestion = input |> isQuestion }

// makeSentence "Hello, is * there?"