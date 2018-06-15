module Game
open Primitives
open Domain
open ItemUse
open GameState
open Actions
open Environment
open GameBehaviors
open Parser
open System
open GameBehaviors

let title = """
,d88~~\ ,e,                         888                 ~~~888~~~                      d8   
8888     "  888-~88e-~88e 888-~88e  888  e88~~8e           888     e88~~8e  Y88b  /  _d88__ 
`Y88b   888 888  888  888 888  888b 888 d888  88b          888    d888  88b  Y88b/    888   
 `Y88b, 888 888  888  888 888  8888 888 8888__888          888    8888__888   Y88b    888   
   8888 888 888  888  888 888  888P 888 Y888    ,          888    Y888    ,   /Y88b   888   
\__88P' 888 888  888  888 888-_88"  888  "88___/           888     "88___/   /  Y88b  "88_/ 
                          888                                                               
"""


// this key is used to open
let keyItem = 
    Items.createInventoryItem
        (ItemId 1)
        "key" "laying in a pile of debris"
        None // no health
        None
        None
        [Behaviors.openExit "After a few minutes of getting the key to fit correctly, the lock releases and the door creakily opens." (ExitId 5);
            Behaviors.takeItem "You pickup a small, crusty key." true]

let typewriter =
    Items.createInventoryItem (ItemId 4) "typewriter" "collecting dust" None None None 
        [Behaviors.openSecretPassage "As you press down hard on one of the keys. The air begins to move around you. Suddenly, a secret passage opens up from within the wall." (ExitId 7);
            Behaviors.takeItem "After several attempts of trying to pick up the typewriter, you realize you don't actually want to carry this thing around." false]

let rock =
    Items.createInventoryItem (ItemId 5) "rock" "just lying around" None None None
        [Behaviors.openSecretPassage "You throw the rock directly at the voice and hear a terrible scream.  Moments later you can hear footsteps running to the east away from you." (ExitId 8);]

// lantern is an item you can take that allows you to see in dark places.
// it can be turned on & off
// it consumes battery life when it's turned on
let lanternItem =
    Items.createInventoryItem
        (ItemId 2) 
        "lantern" "with a full battery"
        (Some (Health(15, 15)))
        (Some Items.SwitchOff)
        None
        [
            Behaviors.loseBattery "Batter Life" 1;
            Behaviors.batteryWarnings "Battery Warning"
                [
                    (0,0, "Lantern's batteries are dead.");
                    (5,5, "Lantern is getting extremely dim.");
                    (10,10, "Lantern is getting dim.");]
            Behaviors.takeItem "You pick up the lantern" true;
        ]

let gold =
    Items.createInventoryItem (ItemId 3) "Gold" "that probably fell out of someones pocket" None None None []

let greenSlime =
    let stats = Player.createStats 1 10 2
    Monster.create 1 "Green Slime" stats (Health (10, 10)) 100
let gruet =
    let stats = Player.createStats 1 14 3
    Monster.create 2 "Gruet" stats (Health (12, 12)) 200
   
let defaultMap =
    [|
        (Environment.create 1 "Origin"
            "A moment ago you were just in bed floating above your mind, dreaming about how to add zebras to spreadsheets.  Now it appears you've awakened in a dimlit room. Many unfamiliar smells lurk around you. There's an old creaky door to the north."
            [Exit.create 1 2 Open North (Steps 2) "Creaky door"]
            [keyItem; lanternItem]
            []
        );
        (Environment.create 2 "Long Hallway, South End"
            "The door opens into what appears to be a really long hallway continuing North. There's no light at the other end."
            [
                Exit.create 2 1 Open South (Steps 2) "Creaky door";
                Exit.create 3 3 Open North (Steps 6) "Dark hallway";]
            []
            [Encounter.create "Green Slime appears and is attacking you!" [greenSlime]]
        );
        (Environment.create 3 "Long Hallway, North End"
            "It gets so dark you have to feel your way around. Thankfully there's nothing too dangerous in your path."
            [ 
                Exit.create 4 2 Open South (Steps 6) "The south end of the hallway";
                Exit.create 5 4 Locked East (Steps 6) "A door with no features, labeled 'Private'"]
            []
            [Encounter.create "A gruet jumps out from the darkness." [gruet]]
        );
        (Environment.create 4 "Office"
            "As the door opens, you begin to see the remnants of an old dusty office.  This place hasn't been used in years. An old typewriter on the desk is missing most of its keys."
            [ Exit.create 6 3 Open West (Steps 6) "Door with no features"; Exit.create 7 5 Hidden East (Steps 2) "Secret Passage"]
            [typewriter]
            []
        );
        (Environment.create 5 "Secret Passage"
            """The path leads downward with considerable gradient. Things turn cold as you hear a voice... 'stoi impul chani, mario.' Frozen, but unable to make out any figures ahead of you, you shout back 'Who's there?'
A few seconds pass, finally a response... 'die!'.  As you fall backward you stumble over a rock.            
            """
            [ Exit.create 7 4 Open West (Steps 2) "Secret entrance"; Exit.create 8 6 Hidden East (Steps 10) "Dark Passage towards the footsteps"]
            [rock]
            []
        );
        (Environment.create 6 "Dark Passage"
            """Is it really a good idea to go chasing after such a terrible, unknown, thing? Probably not, but that hasn't stopped you so far."""
            [ Exit.create 9 5 Open West (Steps 10) "Secret Passage"]
            [gold]
            []
        );
    |]

let player1 = Player.create "P1" (Player.createStats 2 14 3) 15

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
            let item' = update item
            // update item in gamestate
            let inventory' = Environment.updateInventory item' gs.Inventory
            gs |> Inventory.setInventory inventory'
        ) gamestate

let getGameObjectOutputs : GamePart =
    fun gamestate ->
        gamestate.Inventory
        |> getUses isOutputItemUse ItemUse.findGameStateBehavior
        |> List.filter (fun (_, _, update) -> update.IsSome)
        |> List.map (fun (i, (_, itemUse), update) -> ((itemUse,i), update.Value))
        |> List.fold (fun gs ((itemUse, item), update) ->
            update (itemUse, item, gs)
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