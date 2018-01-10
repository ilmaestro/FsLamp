open Microsoft.FSharp.Reflection
open System

type LevelList = seq<int * int * string> // MinExperience, LevelNo, Name

let createLevels numLevels : LevelList =
    seq {
        for i in 0 .. numLevels do
            let minExperience = i * i * i * i
            yield (minExperience, i, (sprintf "Level %i" i))
        }
    |> Seq.sortByDescending (fun (exp,_,_) -> exp)


type Player = {
    Name: string
    Gender: Gender
    Health: int
    Wealth: int
    Experience: int
    BaseAbilities: Map<Ability, int>
    Inventory: Inventory list
    Quests: Quest list
    MapPosition: Position
    TemporaryEffects: Effect list
}
with 
    member this.Level (levelList: LevelList) = 
        levelList 
        |> Seq.filter (fun (exp,_,_) -> this.Experience > exp)
        |> Seq.head

    member this.CurrentAbility ability =
        this.BaseAbilities.Item(ability)
and Gender = 
| Male
| Female
| NonBinary

and Ability =
| Strength
| Defense
| Agility
| Wisdom

and Inventory = 
| Equipment of string * Effect
| Item of string * Effect

and Effect = 
| AbilityEffect of Ability * int * TimeSpan // ability effectAmount lifetime
| NoEffect

and Quest = {
    Name: string
}

and Position = int * int // x,y coords

// function to create and initialize a new level 1 player
let createPlayer name gender wealth experience =
    let rnd = System.Random()
    let health = rnd.Next(10, 20)
    let abilities =
        FSharpType.GetUnionCases typeof<Ability>
        |> Array.map (fun u -> (FSharpValue.MakeUnion(u, [||]) :?> Ability, rnd.Next(10, 20)))
        |> Map.ofArray
    {
        Name = name
        Gender = gender
        Health = health
        Wealth = wealth
        Experience = experience
        BaseAbilities = abilities
        Inventory = []
        Quests = []
        MapPosition = (0, 0)
        TemporaryEffects = []
    }

let updateTemporaryEffects (elapsedTime: int) player =
    let elapsedTimespan = TimeSpan.FromSeconds(float elapsedTime)
    let updatedEffects =
        player.TemporaryEffects
        |> List.map(fun eff ->
            match eff with
            | AbilityEffect (ability, effectAmount, lifetime) ->
                if lifetime.Seconds > elapsedTime 
                then AbilityEffect (ability, effectAmount, lifetime.Subtract(elapsedTimespan))
                else NoEffect
            | NoEffect -> eff
        )
        |> List.filter (fun eff -> eff <> NoEffect) // get rid of noEffects
    { player with TemporaryEffects = updatedEffects }

let createItemWithAbilityEffect name ability effectAmount lifetime  = 
    Item (name, AbilityEffect (ability, effectAmount, lifetime))

let createEquipmentWithAbilityEffect name ability effectAmount lifetime  = 
    Equipment (name, AbilityEffect (ability, effectAmount, lifetime))

// testing
let levels = createLevels 30
let joe = createPlayer "Joe" Male 2000 2500
joe.Level levels
joe.CurrentAbility Defense


