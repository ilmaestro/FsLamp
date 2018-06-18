module Unification
#if INTERACTIVE
#r "../../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#load "bot.fsx"
#load "phrases.fsx"
#endif

open Eliza

// Pattern match on pattern and input:
//  - if both are empty, we succeeded!
//  - if pattern starts with wildcard return the result of one of the
//    following two recursive calls (first one, if it succeeds; second otherwise)
//     * recursively match rest of the pattern (without wildcard)
//       with the original input (and don't assing more words to wildcard)
//     * match all of the pattern (with wildcard)
//       with the rest of the input (assign the first word to the wildcard)
//  - if the pattern & input start with the same word, skip them
//  - otherwise we fail (because the pattern doesn't match input)

let rec unification pattern input (acc: Pattern list list) =
    match pattern, input with
    | [],[] ->
        acc |> List.filter (fun l -> l.Length > 0) |> List.rev |> Some
    | Wildcard :: prest, (Word i) :: irest ->
        match acc with
        | [] ->
            // option 1: end the wildcard, start a new group
            let continueWithoutWildcard = unification prest irest ([Word (i + ".")] :: acc)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard and group
                unification pattern irest ([Word (i + "..")] :: acc)
        | grp :: grpRest ->
            // option 1: end the wildcard, continue group, start a new group
            let continueWithoutWildcard = unification prest irest ([] :: (grp @ [Word (i + "...")]) :: grpRest)

            if continueWithoutWildcard.IsSome then continueWithoutWildcard
            else
                // option 2: continue the wildcard, new group
                unification pattern irest ((grp @ [Word (i + "....")]) :: grpRest)

    | (Word p) :: prest, (Word i) :: irest  when p = i ->
        unification prest irest ([] :: acc)
    | _ -> None

let matchPattern (pattern : Sentence) (input : Sentence) = 
    if pattern.IsQuestion <> input.IsQuestion then None
    else unification pattern.Contents input.Contents []
(matchPattern (makeSentence "open * with * or *") (makeSentence "open the bork creaky door with rusty key or the snarky wrench"))
(matchPattern (makeSentence "open * with *") (makeSentence "open door with key"))


(unification (parseText "a *") (parseText "a b") []) = Some [[Word "b"]]
(unification (parseText "a *") (parseText "b b") []) = None
(unification (parseText "a *") (parseText "a b c") []) = Some [[Word "b"; Word "c"]]
(unification (parseText "* a *") (parseText "a a b c") []) = Some [[Word "a";]; [Word "b"; Word "c"]]
//(unification (parseText "* a") (parseText "a") []) = Some []


// -------------------------------------------------
// TODO: Write the function that will match user input with pattern.
// Because our unification function ignores punctuation, we need to 
// check for it manually here, before calling the unfication function.
// More specifically, if the pattern is a question, then the user 
// input must also be a question. If the pattern is a question and the
// user input is not a question, the two don't match.
// If the user input is a question, the pattern doesn't have to be a 
// question as well.
// After checking if the question condition is satisfied, call the 
// unification function and return the result. 
//
// The function should return None if the question condition is not
// satisfied, or if no unification could be found. Otherwise return
// Some with the identified substitution.
// -------------------------------------------------

(matchPattern (makeSentence "Hello, is * there?") (makeSentence "Hello, is Ryan there?"))

let extractText (pattern : Answer) = 
    pattern 
    |> List.map (fun p ->
        match p with
        | Word w -> w
        | _ -> "")
    |> List.filter (fun x -> x <> "")

let fillAnswer (text : string) (pattern : Answer) =
    pattern 
    |> List.fold (fun result p ->
        match p with
        | Wildcard -> result + " " + text
        | Word w -> result + " " + w ) ""

let doorAnswer = (matchPattern (makeSentence "open * with * or *") (makeSentence "open the bork creaky door with rusty key or the snarky wrench")) |> Option.get 

