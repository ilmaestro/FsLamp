module Primitives

[<Measure>] type meter

[<Measure>] type second

let defaultSpeed = 0.1<meter/second>
let time dist : float<second> = dist / defaultSpeed

type Undefined = exn

// wrappers / primitives
type EnvironmentId = EnvironmentId of int
type ExitId = ExitId of int
type ItemId = ItemId of int
type MonsterId = MonsterId of int
type ExperiencePoint = int
type ExperienceLevel = int
type AttackStat = AttackStat of int
type Damage = Damage of int
type DefenseStat = DefenseStat of int
type BehaviorId = BehaviorId of int
type Power = Power of int
type Description = Description of string

// basic player status
type Health = Health of current: int * max: int
type Experience = Experience of total: ExperiencePoint * level: ExperienceLevel