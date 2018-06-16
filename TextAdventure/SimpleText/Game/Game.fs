module Game
open Domain
open ItemUse
open GameState
open Actions
open Environment
open Parser
open System
open GameItems
open GameMonsters

let title = """
,d88~~\ ,e,                         888                 ~~~888~~~                      d8   
8888     "  888-~88e-~88e 888-~88e  888  e88~~8e           888     e88~~8e  Y88b  /  _d88__ 
`Y88b   888 888  888  888 888  888b 888 d888  88b          888    d888  88b  Y88b/    888   
 `Y88b, 888 888  888  888 888  8888 888 8888__888          888    8888__888   Y88b    888   
   8888 888 888  888  888 888  888P 888 Y888    ,          888    Y888    ,   /Y88b   888   
\__88P' 888 888  888  888 888-_88"  888  "88___/           888     "88___/   /  Y88b  "88_/ 
                          888                                                               
"""




let defaultGamestate map =
    { Player = player1;
        Inventory = [];
        World = { Time = DateTime.Parse("1971-01-01 06:01:42"); Map = map };
        Environment = map.[0];
        GameScene = MainMenu;
        LastCommand = NoCommand;
        Output = Header [title; "Type GO to start the game, or LOAD to start from saved game."]}

let getCommand (parseInput: CommandParser) =
    Console.Write("\n> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand

let getAction gameScene (dispatcher: Command -> GamePart) = 
    let parser = 
        match gameScene with
        | MainMenu -> mainMenuParser
        | OpenExplore -> exploreParser
        | InEncounter _ -> encounterParser
    getCommand parser |> dispatcher

let handleHeader : GamePart =
    fun gamestate ->
        match gamestate.Output with
        | Header log -> log |> List.iter (printfn "%s")
        | _ -> ()
        gamestate

let isUpdateItemUse (_, itemUse) =
    itemUse == Items.LoseLifeOnUpdate

let isOutputItemUse (_, itemUse) =
    itemUse == Items.GetOutputs

let getUses f g (list: Items.InventoryItem list) =
    list 
    |> List.collect (fun i -> i.Behaviors |> List.filter f |> List.map (fun b -> (i, b)))
    |> List.map (fun (i, b) -> (i, b, g b))

let updateGameObjects : GamePart =
    fun gamestate ->
        gamestate.Inventory
        // get all the updatable item uses
        |> getUses isUpdateItemUse findItemUseBehavior
            
        |> List.filter (fun (_, _, update) -> update.IsSome)
        |> List.map (fun (i, (_, itemUse), update) -> ((itemUse,i), update.Value))
        // thread gamestate through all the update functions
        |> List.fold (fun gs (item, update) ->
            // get updated item
            match update item with
            | Ok item' ->
                // update item in gamestate
                let inventory' = Environment.updateInventory item' gs.Inventory
                gs |> Inventory.setInventory inventory'
            | Error _ ->
                gs
        ) gamestate

let getGameObjectOutputs : GamePart =
    fun gamestate ->
        gamestate.Inventory
        |> getUses isOutputItemUse ItemUse.findGameStateBehavior
        |> List.filter (fun (_, _, update) -> update.IsSome)
        |> List.map (fun (i, (_, itemUse), update) -> ((itemUse,i), update.Value))
        |> List.fold (fun gs ((itemUse, item), update) ->
            match update (itemUse, item, gs) with
            | Ok gs' -> gs'
            | Error _ -> gs
        ) gamestate

// loop: Read -> Parse -> Command -> Action -> Print -> Loop
let RunGame
    (dispatcher: Command -> GamePart)
    initialState =
    let rec loop history gamestate =
        // print header
        gamestate |> handleHeader |> ignore

        // get action from dispatcher based on input
        let action = getAction gamestate.GameScene dispatcher

        // execute the action to get next gamestate
        let nextGameState = gamestate |> updateGameObjects |> action  |> getGameObjectOutputs

        // handle output
        match nextGameState.Output with
        | Output log ->
            log |> List.iter (printfn "%s")
            loop (nextGameState :: history) nextGameState
        | Header _ | DoNothing ->
            loop (nextGameState :: history) nextGameState
        | Rollback ->
            printfn "Rolling back..."
            let (oldState, newHistory) =
                match history with
                | _ :: old :: tail  -> (old, tail)
                | _                 -> (gamestate, history)
            loop newHistory oldState
        | ExitGame -> ()
    
    loop [] initialState