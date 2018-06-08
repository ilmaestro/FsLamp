module Player
open Domain
open GameState

let getLevel points =
    match points with
    | p when p < 100 -> 1
    | p when p >= 100 && p < 300 -> 2
    | p when p >= 300 && p < 900 -> 3
    | p when p >= 900 && p < 2700 -> 4
    | _ -> 5

let createExperience points =
    Experience (points, points |> getLevel )

let getExperience gamestate =
    let (Experience(current, level)) = gamestate.Experience
    (current, level)

let addExperience points gamestate =
    let (current, _) = gamestate |> getExperience
    let newExperience = createExperience (current + points)
    gamestate |> setExperience newExperience
    