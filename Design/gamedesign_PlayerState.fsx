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
            + (Player.TotalTemporaryEffectAmount ability this.TemporaryEffects)
            + (Player.TotalEquippedEffectAmount ability this.Inventory)


    static member TotalEquippedEffectAmount ability inventory =
        inventory
        |> List.sumBy (fun inv ->
            match inv with
            | Equipment (_, Equipped, effects) -> 
                Player.TotalPermanentEffectAmount ability effects
            | _ -> 0
        )

    static member TotalTemporaryEffectAmount ability effects =
        effects
        |> List.sumBy (fun te ->
            match te with 
            | TemporaryAbility (_, ab, amount, _) when ab = ability -> 
                amount
            | _ -> 0)

    static member TotalPermanentEffectAmount ability effects =
        effects
        |> List.sumBy (fun te ->
            match te with 
            | PermananentAbility (_, ab, amount) when ab = ability -> 
                amount
            | _ -> 0)

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
| Equipment of string * EquipmentState * Effect list
| Item of string * ItemState * Effect list

and EquipmentState =
| Equipped
| Unequipped

and ItemState =
| Used
| Unused

and Effect = 
| TemporaryAbility of EffectType * Ability * int * TimeSpan // ability effectAmount lifetime
| PermananentAbility of EffectType * Ability * int // ability effectAmount lifetime
| NoEffect

and EffectType =
| Enhancement
| Poisoned
| Drunkeness
| Sleep

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

let createItemWithAbilityEffect name ability effectAmount lifetime  = 
    Item (name, Unused, [TemporaryAbility (Enhancement, ability, effectAmount, lifetime)])

let createEquipmentWithAbilityEffect name ability effectAmount = 
    Equipment (name, Unequipped, [PermananentAbility (Enhancement, ability, effectAmount)])

let addTemporaryEffect temporaryEffect player =
    { player with TemporaryEffects = temporaryEffect :: player.TemporaryEffects }

let addInventory item player =
    { player with Inventory = item :: player.Inventory }

let equipFromInventoryByName name player =
    let newInventory = 
        player.Inventory 
        |> List.map (fun inv ->
            match inv with
            | Equipment (eqName, Unequipped, amount) when eqName = name ->
                Equipment (eqName, Equipped, amount)
            | _ -> inv
        )
    { player with Inventory = newInventory }


let updateTemporaryEffects (elapsedTime: int) player =
    let elapsedTimespan = TimeSpan.FromSeconds(float elapsedTime)
    let updatedEffects =
        player.TemporaryEffects
        |> List.map(fun eff ->
            match eff with
            | TemporaryAbility (effectType, ability, effectAmount, lifetime) ->
                if lifetime > elapsedTimespan 
                then TemporaryAbility (effectType, ability, effectAmount, lifetime.Subtract(elapsedTimespan))
                else NoEffect
            | _ -> eff
        )
        |> List.filter (fun eff -> eff <> NoEffect) // get rid of noEffects
    { player with TemporaryEffects = updatedEffects }

// testing
let levels = createLevels 30
let joe = 
    createPlayer "Joe" Male 2000 2500
    |> addInventory (createEquipmentWithAbilityEffect "Sword of Destiny" Strength 5)
    |> addTemporaryEffect (TemporaryAbility (Poisoned, Strength, -2, TimeSpan.FromMinutes(5.)))
    |> equipFromInventoryByName "Sword of Destiny"

joe.Level levels
joe.CurrentAbility Defense
joe.CurrentAbility Strength

joe |> updateTemporaryEffects 300

// questions?
// can inventory items have multiple effects? yes
// effects 