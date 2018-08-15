module Player
open Primitives
open Domain
open GameState

let getLevel points =
    match points with
    | p when p < 100 -> 1
    | p when p >= 100 && p < 300 -> 2
    | p when p >= 300 && p < 900 -> 3
    | p when p >= 900 && p < 2700 -> 4
    | _ -> 5

let create name stats totalHealth =
    { Name = name; Stats = stats; Health = Health (totalHealth,totalHealth); Experience = Experience (0, 1);  }

let createExperience points =
    Experience (points, points |> getLevel )

let setExperience experience (player: Player) =
    {player with Experience = experience }

let addExperience points (player: Player) =
    let (Experience(current, _)) = player.Experience
    player 
    |> setExperience (createExperience (current + points))
    
let setHealth health (player: Player) =
    {player with Health = health}

let checkGameOver gamestate =
    if gamestate.Player.Health |> isAlive then
        gamestate
    else
        gamestate
        |> Scene.setScene MainMenu
        |> Output.setOutput GameOver

let playerStatus (player: Player) =
    let (AttackStat attack) = player.Stats.Attack
    let (DefenseStat defense) = player.Stats.Defense
    let (Damage damage) = player.Stats.Damage
    let (Health (current, max)) = player.Health
    let (Experience (total, level)) = player.Experience
    sprintf """
## STATS
```FloralWhite
Health:     %i / %i     Attack:     %i
Level:      %i          Defense:    %i
Experience: %i          Damage:     %i
```""" current max attack level defense total damage

module Rolls =
    let private rnd = System.Random()

    let d20Roll () =
        rnd.Next(1, 20)

    let d4Roll () =
        rnd.Next(1, 4)

    let roll20 () = 20