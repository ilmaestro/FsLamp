module Environment
open Domain
open GameState

module Environment =
    let create id name description exits items environmentItems =
        { Id = EnvironmentId id; Name = name; Description = description; Exits = exits; InventoryItems = items; EnvironmentItems = environmentItems }

    let findById id gamestate =
        gamestate.World.Map
        |> Array.find (fun env -> env.Id = id)

module Item =
    let createEnvironmentItem name uses =
        EnvironmentItem { Name = name; Uses = uses }

    let createInventoryItem name description uses =
        InventoryItem { Name = name; Description = description; Uses = uses; Behaviors = [] }

    let createTemporaryItem name description uses lifetime behaviors =
        TemporaryItem ({ Name = name; Description = description; Uses = uses;  Behaviors = behaviors }, lifetime)

    let createAttackItem name description damage behaviors =
        AttackItem { Name = name; Description = description; Damage = damage; Behaviors = behaviors }

    let addItem item gamestate =
        { gamestate with Inventory = item :: gamestate.Inventory }

    let removeItemFromEnvironment item gamestate =
        let environment = {gamestate.Environment with InventoryItems = gamestate.Environment.InventoryItems |> List.filter (fun i -> i <> item) }
        { gamestate with Environment = environment}

    let addItemToEnvironment item gamestate =
        let environment = {gamestate.Environment with InventoryItems = item :: gamestate.Environment.InventoryItems }
        { gamestate with Environment = environment}

    let inventoryItemName item =
        match item with
        | InventoryItem props -> props.Name
        | TemporaryItem (props, _) -> props.Name
        | AttackItem props -> props.Name

    let inventoryItemProps item =
        match item with
        | InventoryItem props -> (props.Name, props.Description)
        | TemporaryItem (props, _) -> (props.Name, props.Description)
        | AttackItem props -> (props.Name, props.Description)

    let inventoryItemBehaviors item =
        match item with
        | InventoryItem props -> props.Behaviors
        | TemporaryItem (props, _) -> props.Behaviors
        | AttackItem props -> props.Behaviors

    let environmentItemDescription item =
        match item with
        | EnvironmentItem props -> props.Name
        | _ -> ""

module Exit =
    let create id environmentId exitState direction distance description =
        { Id = ExitId id; Target = EnvironmentId environmentId; ExitState = exitState; Direction = direction; 
            Distance = distance; Description = description }

    let updateEnvironment exit gamestate =
        let exits = gamestate.Environment.Exits |> List.map (fun e -> if e.Id = exit.Id then exit else e)
        let environment = {gamestate.Environment with Exits = exits }
        { gamestate with Environment = environment} 

    let openExit exit gamestate =
        gamestate
        |> updateEnvironment exit
        |> updateWorldEnvironment

    let tryOpen id env =
        env.Exits 
        |> List.tryFind (fun e -> e.Id = id && e.ExitState <> Open)
        |> Option.map (fun e -> { e with ExitState = Open })

    let find id env =
        env.Exits |> List.find (fun e -> e.Id = id)

module Uses =
    let find uses environment =
        uses
        |> List.map(fun u -> 
            match u with
            | Unlock (exitId, _)
            | Unhide (exitId, _) ->
                environment.Exits 
                |> List.tryFind (fun e -> e.Id = exitId && e.ExitState <> Open)
                |> Option.map (fun _ -> u)
        )
        |> List.choose id
        |> List.tryHead

module Monster =
    let create id name defense attack damage health experience =
        { Id = MonsterId id; Name = name; Defense = defense; Attack = attack; Damage = damage; Health = health; ExperiencePoints = experience}

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

    let remove gamestate =
        let items = 
            gamestate.Environment.EnvironmentItems 
            |> List.filter (fun i -> 
                match i with 
                | Encounter _ -> false
                | _ -> true)

        let environment = {gamestate.Environment with EnvironmentItems = items }
        {gamestate with Environment = environment }

    let updateMonster (encounter: EncounterProperties) (monster: Monster) =
        let monsters = encounter.Monsters |> List.map (fun m -> if m.Name = monster.Name then monster else m)
        {encounter with Monsters = monsters}

    let updateEncounter (encounter: EncounterProperties) gamestate =
        let environmentItems = 
            gamestate.Environment.EnvironmentItems 
            |> List.map (fun i ->
                match i with
                | Encounter e when e.Description = encounter.Description -> Encounter encounter
                | _ -> i)
        let environment = { gamestate.Environment with EnvironmentItems = environmentItems}
        gamestate |> setEnvironment environment

    let checkEncounter gamestate =  
        match find gamestate.Environment with
        | Some encounter ->
            gamestate
            |> addOutput (sprintf "*** %s ***" encounter.Description)
            |> setScene (InEncounter encounter)
        | None -> gamestate

    let endEncounter gamestate =
        gamestate
        |> remove
        |> updateWorldEnvironment

    let findAMonster encounter =
        encounter.Monsters |> List.filter (fun m -> m.Health |> isAlive) |> List.tryHead

    let checkForMonsters encounter =
        encounter.Monsters |> List.exists (fun m -> m.Health |> isAlive)
