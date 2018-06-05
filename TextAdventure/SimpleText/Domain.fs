module Domain
open System
open System.ComponentModel.Design

type Undefined = exn
type Player = Player of string
type Health = Health of current: float * max: float
type Experience = Experience of total: int * level: int
type Environment = {
    Time: DateTime
}

type Input =
| NoInput
| Wait of TimeSpan
| Status
| Help
| Exit

type Output = 
| Output of string list
| Empty

type GameState = {
    Player: Player
    Health: Health
    Experience: Experience
    Environment: Environment
    Input: Input
    Output: Output
}

type GameError = Undefined

type Command<'a> = GameState -> 'a -> Result<GameState, GameError>

type GamePart = GameState -> GameState
type InputParser = string -> Input option