module PlayerState

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
| Artifact of string

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

and Position = int * int // x,y coords

and Action = 
| Using of Inventory
| Say of string

// quest = series of tasks that must be completed by triggering specific events
and Quest = {
    Name: string
    QuestState: QuestState
    QuestTasks: QuestTask list
}

and QuestState = 
| Inactive
| Active
| Succeeded
| Failed

and QuestTask = {
    TaskType: TaskType
    Name: string
    TaskState: TaskState
    UpdateState: (Player * Quest * Action list) -> TaskState
}
and TaskType =
| Find of Inventory
| Approach of Position
| Perform of Action

and TaskState =
| Incomplete
| Complete
| CompleteAtCurrentMoment

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

let createItemWithAbilityEffect name abilityEffects  = 
    let effects = abilityEffects |> List.map (fun (effectType, ability, effectAmount, lifetime) -> TemporaryAbility (effectType, ability, effectAmount, lifetime))
    Item (name, Unused, effects)

let createEquipmentWithAbilityEffect name abilityEffects = 
    let effects = abilityEffects |> List.map (fun (ability, effectAmount) -> PermananentAbility (Enhancement, ability, effectAmount))
    Equipment (name, Unequipped, effects)

let addTemporaryEffect temporaryEffect player =
    { player with TemporaryEffects = temporaryEffect :: player.TemporaryEffects }

let addInventory item player =
    { player with Inventory = item :: player.Inventory }

let setPosition position player = 
    { player with MapPosition = position}

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


let elapseTemporaryEffects (elapsedTime: int) player =
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

let equipmentMap = 
    dict [
        ("SwordOfDestiny", createEquipmentWithAbilityEffect "Sword of Destiny" [(Strength, 5)]);
        ("LeatherSheild", createEquipmentWithAbilityEffect "Leather Shield" [(Defense, 2)]);
        ("LeatherBoots", createEquipmentWithAbilityEffect "Leather Boots" [(Defense, 1); (Agility, 1)]);
        ("LeatherGloves", createEquipmentWithAbilityEffect "Leather Boots" [(Defense, 1);]);
    ] 

let itemMap =
    dict [
        ("SpiderVenom", createItemWithAbilityEffect "Spider Venom" [(Poisoned, Strength, -2, TimeSpan.FromMinutes(5.))])
    ]

let artifactMap =
    dict [
        ("CrystalSkull", Artifact "Crystal Skull");
        ("BookOfDead", Artifact "Book of the Dead");
    ]

let joe = 
    createPlayer "Joe" Male 2000 2500
    |> addInventory (equipmentMap.["SwordOfDestiny"])
    |> addInventory (itemMap.["SpiderVenom"])
    |> addInventory (artifactMap.["CrystalSkull"])
    |> equipFromInventoryByName "Sword of Destiny"

let runTest (player: Player) levels =
    printfn "Player level: %A" (player.Level levels)
    printfn "Player defense: %i" (player.CurrentAbility Defense)
    printfn "Player strength: %i" (player.CurrentAbility Strength)
    printfn "remov temp effects: %A" (player |> elapseTemporaryEffects 300)

// questions?
// can inventory items have multiple effects? yes
// effects 