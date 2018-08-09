module Environment
open Primitives
open Domain
open Items

type Environment = {
    Id: EnvironmentId
    Name: string
    Description: string
    Exits: Exit list
    InventoryItems: InventoryItem list
    EnvironmentItems: EnvironmentItem list
    LightSource: InventoryItem option
}
with
    member x.Describe () =
        sprintf "%s" x.Description

module Environment =
    let create id name description exits items environmentItems lightsource =
        { Id = EnvironmentId id; Name = name; Description = description; Exits = exits; InventoryItems = items; EnvironmentItems = environmentItems; LightSource = lightsource }

    let updateInventory (item: InventoryItem) (inventory: InventoryItem list) =
        inventory
        |> List.map (fun i -> if i.Id = item.Id then item else i)

module Exit =
    let create id environmentId exitState direction distance description =
        { Id = ExitId id; Target = EnvironmentId environmentId; ExitState = exitState; Direction = direction; 
            Distance = distance; Description = description }

    let tryOpen id env =
        env.Exits 
        |> List.tryFind (fun e -> e.Id = id && e.ExitState <> ExitState.Open)
        |> Option.map (fun e -> { e with ExitState = ExitState.Open })

    let find id env =
        env.Exits |> List.find (fun e -> e.Id = id)


module Monster =
    let create id name stats health experience =
        { Id = MonsterId id; Name = name; Stats = stats; Health = health; ExperiencePoints = experience}

    let isAlive (monster: Monster) =
        monster.Health |> Domain.isAlive

    let setHealth health (monster: Monster) =
        {monster with Health = health}

module Encounter =
    let create description monsters =
        Encounter { Description = description; Monsters = monsters; }

    let find environment =
        environment.EnvironmentItems
        |> List.map (fun i ->
            match i with
            | Encounter e -> Some e
            | _ -> None)
        |> List.choose id
        |> List.tryHead



    let findAMonster encounter =
        encounter.Monsters |> List.filter (fun m -> m.Health |> isAlive) |> List.tryHead

    let checkForMonsters encounter =
        encounter.Monsters |> List.exists (fun m -> m.Health |> isAlive)
