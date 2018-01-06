open Microsoft.FSharp.Reflection

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
    Equipment: Equipment list
    Items: Item list
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

and Equipment = {
    Name: string
    Effect: Effect
}
and Item = {
    Name: string
    Effect: Effect
}
and Quest = {
    Name: string
}

and Effect = {
    Name: string
    Ability: Ability
    Add: int
    Lifetime: int
}
and Position = int * int // x,y coords

// function to create and initialize a new level 1 player
let createPlayer name gender =
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
        Wealth = 0
        Experience = 2445
        BaseAbilities = abilities
        Equipment = []
        Items = []
        Quests = []
        MapPosition = (0, 0)
        TemporaryEffects = []
    }

// testing
let levels = createLevels 30
let joe = createPlayer "Joe" Male
joe.Level levels
joe.CurrentAbility Defense


