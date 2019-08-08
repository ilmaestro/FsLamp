module FsLamp.Game.Characters
open FsLamp.Core.Primitives
open FsLamp.Core.Domain
open FsLamp.Core

let player1 = 
    Player.create "P1" (createStats 2 14 3) 15

let greenSlime =
    let stats = createStats 1 10 2
    Monster.create 1 "Green Slime" stats (Health (10, 10)) 100

let grue =
    let stats = createStats 1 14 3
    Monster.create 2 "Grue" stats (Health (12, 12)) 200
   