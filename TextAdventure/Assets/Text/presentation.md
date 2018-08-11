# FsLamp Presentation

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
- Natural Language Parsing
- GameParts
- Behaviors
- Making a game
- Where to from here?

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

FsLamp uses LUIS to parse user input into intents.  The LUIS outputs look like:

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

Example of how to convert a LUIS result into a command

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

### Basic Info

---

### Configuration

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

The idea of a gamepart is simple, a function that takes GameState and returns GameState.  

```fsharp
type GamePart = GameState -> GameState
```

By using this as a common pattern, it becomes very easy to compose functions that manipulate GameState.

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
 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

Game behaviors are what give the player the ability to interact with components of the game.

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