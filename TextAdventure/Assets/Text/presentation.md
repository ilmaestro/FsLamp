```SkyBlue
   _______  _______    ___      _______  __   __  _______ 
  |       ||       |  |   |    |   _   ||  |_|  ||       |
  |    ___||  _____|  |   |    |  |_|  ||       ||    _  |
  |   |___ | |_____   |   |    |       ||       ||   |_| |
  |    ___||_____  |  |   |___ |       ||       ||    ___|
  |   |     _____| |  |       ||   _   || ||_|| ||   |    
  |___|    |_______|  |_______||__| |__||_|   |_||___|    
```

![Hammer](../Assets/smallhammer.png)

By Ryan Kilkenny - Portland F# Meetup

FsLamp is a text adventure engine written in F#. It was developed out of a learning project meant to improve F# skills, game design skills, and hopefully pave the path towards future game development and natural language processing. In this talk we'll take a look at the game engine implementation and learn how to write interactive fiction games in F#.

---

```SpringGreen
 _     _  __   __  _______    _______  __   __    ___   ______  
| | _ | ||  | |  ||       |  |   _   ||  |_|  |  |   | |      | 
| || || ||  |_|  ||   _   |  |  |_|  ||       |  |   | |___   | 
|       ||       ||  | |  |  |       ||       |  |   |   __|  | 
|       ||       ||  |_|  |  |       ||       |  |   |  |_____| 
|   _   ||   _   ||       |  |   _   || ||_|| |  |   |    __    
|__| |__||__| |__||_______|  |__| |__||_|   |_|  |___|   |__|   
```

![farmer](../Assets/FarmerSprite.png)

Ryan Kilkenny is a multi-paradigm polyglot primarily living in the .NET world. He's been an F# enthusiast since 2013 and interested in game development since playing Space Invaders on a Commodore VIC-20 in 1985. He is currently employed by Banfield Pet Hospital and enjoys working within a team of extremely talented IT professionals to make the world better for pets.

---

```FloralWhite
 _______  _______  _______  __    _  ______   _______ 
|   _   ||       ||       ||  |  | ||      | |   _   |
|  |_|  ||    ___||    ___||   |_| ||  _    ||  |_|  |
|       ||   | __ |   |___ |       || | |   ||       |
|       ||   ||  ||    ___||  _    || |_|   ||       |
|   _   ||   |_| ||   |___ | | |   ||       ||   _   |
|__| |__||_______||_______||_|  |__||______| |__| |__|
```

- The Game Loop
- State
- Natural Language Parsing
- GameParts
- Items
- Behaviors
- Why F#?

![alien](../Assets/alien_2.png)

---

```SkyBlue
 _______  _______  __   __  _______    ___      _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |   |    |       ||       ||       |
|    ___||  |_|  ||       ||    ___|  |   |    |   _   ||   _   ||    _  |
|   | __ |       ||       ||   |___   |   |    |  | |  ||  | |  ||   |_| |
|   ||  ||       ||       ||    ___|  |   |___ |  |_|  ||  |_|  ||    ___|
|   |_| ||   _   || ||_|| ||   |___   |       ||       ||       ||   |    
|_______||__| |__||_|   |_||_______|  |_______||_______||_______||___|    
```

```SpringGreen
Input -> Action -> Output -> Print -> Loop
```

---

```SkyBlue
 _______  _______  __   __  _______    ___      _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |   |    |       ||       ||       |
|    ___||  |_|  ||       ||    ___|  |   |    |   _   ||   _   ||    _  |
|   | __ |       ||       ||   |___   |   |    |  | |  ||  | |  ||   |_| |
|   ||  ||       ||       ||    ___|  |   |___ |  |_|  ||  |_|  ||    ___|
|   |_| ||   _   || ||_|| ||   |___   |       ||       ||       ||   |    
|_______||__| |__||_|   |_||_______|  |_______||_______||_______||___|    
```

```fsharp
let RunGame actionResolver initialState =
    let rec loop history gamestate =
        // get action from dispatcher based on input
        let action = actionResolver gamestate.GameScene

        // execute the action to get next gamestate
        let nextGameState = gamestate |> updateGameObjects |> action  |> getGameObjectOutputs

        // update screen
        Console.update nextGameState

        // handle output
        match nextGameState.Output with
        | Output _ | DoNothing ->
            loop (nextGameState :: history) nextGameState
        | ExitGame ->
            ()

    // initial screen update
    Console.update initialState

    // start game loop
    loop [] {initialState with Output = DoNothing }
```

---

```SkyBlue
 _______  _______  __   __  _______    _______  _______  _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |       ||       ||   _   ||       ||       |
|    ___||  |_|  ||       ||    ___|  |  _____||_     _||  |_|  ||_     _||    ___|
|   | __ |       ||       ||   |___   | |_____   |   |  |       |  |   |  |   |___ 
|   ||  ||       ||       ||    ___|  |_____  |  |   |  |       |  |   |  |    ___|
|   |_| ||   _   || ||_|| ||   |___    _____| |  |   |  |   _   |  |   |  |   |___ 
|_______||__| |__||_|   |_||_______|  |_______|  |___|  |__| |__|  |___|  |_______|
```

 - One big object graph

```fsharp
type GameState = {
    Player: Player
    Inventory: InventoryItem list
    Environment: Environment
    World: World
    GameScene: GameScene
    LastCommand: Command
    Output: Output
}
```

---

```SkyBlue
 _______  _______  __   __  _______    _______  _______  _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |       ||       ||   _   ||       ||       |
|    ___||  |_|  ||       ||    ___|  |  _____||_     _||  |_|  ||_     _||    ___|
|   | __ |       ||       ||   |___   | |_____   |   |  |       |  |   |  |   |___ 
|   ||  ||       ||       ||    ___|  |_____  |  |   |  |       |  |   |  |    ___|
|   |_| ||   _   || ||_|| ||   |___    _____| |  |   |  |   _   |  |   |  |   |___ 
|_______||__| |__||_|   |_||_______|  |_______|  |___|  |__| |__|  |___|  |_______|
```

- Easy to serialize

```fsharp
module IO =
    let saveGameState filename gamestate =
        let json = JsonConvert.SerializeObject(gamestate)
        System.IO.File.WriteAllText(filename, json)

    let loadGameState filename =
        let json = System.IO.File.ReadAllText(filename)
        JsonConvert.DeserializeObject<GameState>(json)
```

---

```SkyBlue
 _______  _______  __   __  _______  _______  _______  ______    _______ 
|       ||   _   ||  |_|  ||       ||       ||   _   ||    _ |  |       |
|    ___||  |_|  ||       ||    ___||    _  ||  |_|  ||   | ||  |_     _|
|   | __ |       ||       ||   |___ |   |_| ||       ||   |_||_   |   |  
|   ||  ||       ||       ||    ___||    ___||       ||    __  |  |   |  
|   |_| ||   _   || ||_|| ||   |___ |   |    |   _   ||   |  | |  |   |  
|_______||__| |__||_|   |_||_______||___|    |__| |__||___|  |_|  |___|  
```

- Function that takes __GameState__ and returns __GameState__

```fsharp
type GamePart = GameState -> GameState
```

---

```SkyBlue
 _______  _______  __   __  _______  _______  _______  ______    _______ 
|       ||   _   ||  |_|  ||       ||       ||   _   ||    _ |  |       |
|    ___||  |_|  ||       ||    ___||    _  ||  |_|  ||   | ||  |_     _|
|   | __ |       ||       ||   |___ |   |_| ||       ||   |_||_   |   |  
|   ||  ||       ||       ||    ___||    ___||       ||    __  |  |   |  
|   |_| ||   _   || ||_|| ||   |___ |   |    |   _   ||   |  | |  |   |  
|_______||__| |__||_|   |_||_______||___|    |__| |__||___|  |_|  |___|  
```

- easy to compose

```fsharp
let takeItem item : GamePart =
    fun gamestate ->
        gamestate
        |> Environment.removeItemFromEnvironment item
        |> Inventory.addItem item
        |> World.updateWorldEnvironment
        |> Output.setOutput (Output (getSuccessOutputs item))
```

---

```SkyBlue
 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

- __Parsing__ takes user input and translates it into a command
- a command can then be __dispatched__ to an __action__

---

```SkyBlue
 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

```SpringGreen
Get Input -> LUIS -> Build Command -> Dispatch Action
```

```fsharp
type CommandParser = string -> Command option
```

---

```SkyBlue
 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

- Get input

```fsharp
let getCommand (parseInput: CommandParser) =
    Console.Write("\n> ")
    let readline = Console.ReadLine()
    match readline |> parseInput with
    | Some command -> command
    | None -> 
        printfn "I don't understand %s." readline
        NoCommand
```

---

```SkyBlue
 ______   ___   _______  _______  _______  _______  _______  __   __  _______  ______   
|      | |   | |       ||       ||   _   ||       ||       ||  | |  ||       ||    _ |  
|  _    ||   | |  _____||    _  ||  |_|  ||_     _||       ||  |_|  ||    ___||   | ||  
| | |   ||   | | |_____ |   |_| ||       |  |   |  |       ||       ||   |___ |   |_||_ 
| |_|   ||   | |_____  ||    ___||       |  |   |  |      _||       ||    ___||    __  |
|       ||   |  _____| ||   |    |   _   |  |   |  |     |_ |   _   ||   |___ |   |  | |
|______| |___| |_______||___|    |__| |__|  |___|  |_______||__| |__||_______||___|  |_|
```

```fsharp
let dispatch command : GamePart =
    fun gamestate ->
        let action = 
            match command with
            // open explore
            | NoCommand                         -> message "Nothing to do."
            | Status                            -> status
            | Wait ts                           -> Explore.wait ts
            | Exit                              -> Explore.exitGame
            | Help                              -> Explore.help
            | Move dir                          -> Explore.move dir
            | Look                              -> Explore.look
            | LookIn itemName                   -> Explore.lookIn itemName
            | Undo                              -> Explore.undo
            | Take itemName                     -> Explore.take itemName
            | TakeFrom (targetName, itemName)   -> Explore.takeFrom targetName itemName
            | Drop itemName                     -> Explore.drop itemName
            | Use itemName                      -> Explore.useItem itemName
            | UseWith (targetName, itemName)    -> Explore.useItemOn targetName itemName
            | SwitchItemOn itemName             -> Explore.switch itemName SwitchState.SwitchOn
            | SwitchItemOff itemName            -> Explore.switch itemName SwitchState.SwitchOff
            | PutItem (sourceName, targetName)  -> Explore.put sourceName targetName
            | SaveGame                          -> Explore.save getSaveDataFilename
            | Read itemName                     -> Explore.readItem itemName
            // main menu
            | NewGame                           -> MainMenu.startGame
            | LoadGame                          -> MainMenu.loadGame >> MainMenu.startGame
            // in encounter
            | Attack                            -> InEncounter.attack
            | Run                               -> InEncounter.run

        {gamestate with LastCommand = command } |> action
```

---

```SkyBlue
 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

Language Understanding LUIS <https://www.luis.ai/home>. A machine learning-based service to build natural language into apps, bots, and IoT devices.

- Create Intents: Move, Look, Examine, TurnOn, TurnOff
- Create Entities: Item, Operation
- Define utterances: "go to the north", "open the door with key"
- Improve matching with Patterns: "open {item:target} with {item:source}"
- Train
- Publish

FsLamp on LUIS
<https://www.luis.ai/applications/a658f77c-b290-4a81-9ed8-404c95537c9d/versions/0.1/build/intents>

---

```SkyBlue
 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

The LUIS outputs look like:

```json
{
   "query": "Book me a flight to Cairo",
   "topScoringIntent": {
       "intent": "BookFlight",
       "score": 0.9887482
   },
   "intents": [
       {
           "intent": "BookFlight",
           "score": 0.9887482
       }
   ],
   "entities": [
       {
           "entity": "cairo",
           "type": "Location",
           "startIndex": 20,
           "endIndex": 24,
           "score": 0.956781447
       }
   ]
}
```

---

```SkyBlue
 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

Example of how to parse a LUIS result

```fsharp
match query.TopScoringIntent with
| { Intent = "Move";} ->
    match query.Entities with
    | entity :: _ when entity.Type = "Direction" && entity.Score > 0.5 ->
        entity.Entity |> Direction.Parse |> Option.map Move
    | _ -> None
| { Intent = "SwitchOn"}
| { Intent = "SwitchOff"} ->
    match query.Entities with
    | [e1; e2] when e1.Type = "Item" && e2.Type = "SwitchOperation" ->
        if e2.Entity = "on" 
        then SwitchItemOn (e1.Entity) |> Some
        else SwitchItemOff (e1.Entity) |> Some
    | [e2; e1] when e2.Type = "SwitchOperation" && e1.Type = "Item" ->
        if e2.Entity = "on"
        then SwitchItemOn (e1.Entity) |> Some
        else SwitchItemOff (e1.Entity) |> Some
    | _ -> None
```

---

```SkyBlue
 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

- optional properties used over inherited types
  - keeps serialization simple
  - new properties will likely need to be added, which may make increase maintenance
- list of behaviors

---

```SkyBlue
 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

```fsharp
type InventoryItem = {
    Id: ItemId
    Name: string
    Description: string
    Health: Health option
    SwitchState: SwitchState option
    Stats: Stats option
    Contains: (InventoryItem list) option
    Behaviors: (Description * ItemUse) list
}
```

---

```SkyBlue
 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

```fsharp
let theSun =
    createBasicItem "sun" "is shining overhead." [(Description "ball of fire", ProvidesLight)]

```

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

- Game behaviors give the player the ability to interact with items in the game
- Each behavior assigned an Id and stored in a runtime cache
- Behavior Id's get persisted in GameState as part of an item

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

```fsharp
type UpdateGameStateBehavior =
    (ItemUse * InventoryItem * GameState) -> Result<GameState, UpdateGameStateFailure>

type UpdateItemBehavior =
    (ItemUse * InventoryItem) -> Result<InventoryItem,UpdateItemFailure>
```

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

- Behaviors are saved to a cache at runtime

```fsharp
let mutable private gameStateBehaviorCache : Map<(Description * ItemUse),UpdateGameStateBehavior> =
    Map.empty

let addGameStateBehavior id b =
    gameStateBehaviorCache <- gameStateBehaviorCache.Add(id, b)
    id

let findGameStateBehavior id =
    gameStateBehaviorCache.TryFind id
```

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

Define behaviors

```fsharp
    let updateHealthBehavior f : UpdateItemBehavior=
        fun (itemUse: ItemUse, item: InventoryItem) ->
            match itemUse, item.Health, item.SwitchState with
            | LoseLifeOnUpdate, Some h, Some switch ->
                if switch = SwitchOn then { item with Health = Some (f h)} else item
                |> Ok
            | LoseLifeOnUpdate, Some h, None ->
                { item with Health = Some (f h)}
                |> Ok
            | _ -> item |> failItemUpdate "Item use not supported"

    let loseBattery description amount =
        ItemUse.addItemUseBehavior
            (Description description, Items.LoseLifeOnUpdate)
            (updateHealthBehavior (fun (Health (life,total)) -> Health(life - amount,total)))
```

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

Add the behaviors to items

```fsharp
let lantern =
    createInventoryItem
        "lantern" "with a full battery"
        (Some (Health(150, 150)))
        (Some Items.SwitchOff)
        None
        None
        [
            Behaviors.loseBattery "Batter Life" 1; // per turn
            Behaviors.batteryWarnings "Battery Warning"
                [
                    (0,0, "Lantern's batteries are dead.");
                    (10,10, "Lantern is getting extremely dim.");
                    (20,20, "Lantern is getting dim.");]
            Behaviors.takeItem "You pick up the lantern." true;
            Behaviors.turnOnOff "You turn the switch.";
            (Description "Light", ProvidesLight);
        ]
```

---

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

Add items to Environments

```fsharp
let origin =
    (Environment.create 1 "Origin"
            (Utility.readTextAsset "1_Intro.md")
            [Exit.create 1 2 Open North (Steps 2) "Creaky door"]
            [keyItem (ExitId 5); lanternItem; mailbox;]
            []
            (Some ambientLight)
        )
```

```SkyBlue
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

- Serialized nicely

```json
"Behaviors": [
    {
        "Item1": {
            "Case": "Description",
            "Fields": [
                "After a few minutes of getting the key to fit correctly, the lock releases and the door creakily opens."
            ]
        },
        "Item2": {
            "Case": "OpenExit",
            "Fields": [
                {
                    "Case": "ExitId",
                    "Fields": [
                        5
                    ]
                }
            ]
        }
    },
    {
        "Item1": {
            "Case": "Description",
            "Fields": [
                "You pickup the small, crusty key."
            ]
        },
        "Item2": {
            "Case": "CanTake",
            "Fields": [
                true
            ]
        }
    }
]
```

---

```SkyBlue
 _     _  __   __  __   __  ______  
| | _ | ||  | |  ||  | |  ||      | 
| || || ||  |_|  ||  |_|  ||___   | 
|       ||       ||       |  __|  | 
|       ||       ||_     _| |_____| 
|   _   ||   _   |  |   |     __    
|__| |__||__| |__|  |___|    |__|   
```

Does F# work well for a text adventure game?  Yes, in my opinion.

- Domain driven design
- Immutable
- Multi-paradigm
  - blend functional and imperatve coding
  - balance performance with maintainability
- .NET Core / Cross-platform
- lots of nuget packages

---

```Firebrick
 _______  __   __  _______  __    _  ___   _    __   __  _______  __   __ 
|       ||  | |  ||   _   ||  |  | ||   | | |  |  | |  ||       ||  | |  |
|_     _||  |_|  ||  |_|  ||   |_| ||   |_| |  |  |_|  ||   _   ||  | |  |
  |   |  |       ||       ||       ||      _|  |       ||  | |  ||  |_|  |
  |   |  |       ||       ||  _    ||     |_   |_     _||  |_|  ||       |
  |   |  |   _   ||   _   || | |   ||    _  |    |   |  |       ||       |
  |___|  |__| |__||__| |__||_|  |__||___| |_|    |___|  |_______||_______|
```

![ufo](../Assets/UFO_1.png)

Check out FsLamp on Github <https://github.com/ilmaestro/FsLamp>
