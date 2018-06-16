module GameMonsters
open Primitives
open Domain
open Environment

let player1 = 
    Player.create "P1" (createStats 2 14 3) 15

let greenSlime =
    let stats = createStats 1 10 2
    Monster.create 1 "Green Slime" stats (Health (10, 10)) 100
let gruet =
    let stats = createStats 1 14 3
    Monster.create 2 "Gruet" stats (Health (12, 12)) 200
   