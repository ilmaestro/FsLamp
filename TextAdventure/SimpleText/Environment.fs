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
        InventoryItem { Name = name; Description = description; Uses = uses }

    let createTemporaryItem name description uses lifetime =
        TemporaryItem ({ Name = name; Description = description; Uses = uses }, lifetime)


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
        | _ -> failwith "not an inventory item."

    let inventoryItemProps item =
        match item with
        | InventoryItem props -> props
        | _ -> failwith "not an inventory item."

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
    let create id name level health experience =
        { Id = MonsterId id; Name = name; Level = level; Health = health; ExperiencePoints = experience}

    let isAlive (monster: Monster) =
        monster.Health |> Domain.isAlive

module Encounter =
    let create description monsters state =
        Encounter { Description = description; Monsters = monsters;  EncounterState = state; }

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

    let getNextEncounterState encounter =
        //in order to win the encounter, all monsters must not be alive
        let noMonstersAlive = not <| (encounter.Monsters |> List.forall Monster.isAlive)

        match encounter.EncounterState with
        | NotStarted -> InProgress
        | InProgress when noMonstersAlive -> Complete
        | _ -> InProgress

    let checkEncounter gamestate =  
        match find gamestate.Environment with
        | Some encounter ->
            gamestate
            |> addOutput (sprintf "!! %s !!" encounter.Description)
            |> setScene (InEncounter encounter)
        | None -> gamestate

    let endEncounter gamestate =
        gamestate
        |> remove
        |> updateWorldEnvironment

