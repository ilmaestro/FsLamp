```SpringGreen

    Ryan Kilkenny - Open F# Conference - 09/26/2019




```

```SkyBlue
 _______  _______    ___      _______  __   __  _______ 
|       ||       |  |   |    |   _   ||  |_|  ||       |
|    ___||  _____|  |   |    |  |_|  ||       ||    _  |
|   |___ | |_____   |   |    |       ||       ||   |_| |
|    ___||_____  |  |   |___ |       ||       ||    ___|
|   |     _____| |  |       ||   _   || ||_|| ||   |    
|___|    |_______|  |_______||__| |__||_|   |_||___|    
```

---

![farmer](../Assets/FarmerSprite.png)

- Love retro games and hardware
- Sr Technical Lead, Banfield Pet Hospital
- F# projects peppered throughout my 4 yr tenure at Banfield
- Currently attempting F# domain models in C# microservices

```SpringGreen
 _     _  __   __  _______    _______  __   __    ___   ______  
| | _ | ||  | |  ||       |  |   _   ||  |_|  |  |   | |      | 
| || || ||  |_|  ||   _   |  |  |_|  ||       |  |   | |___   | 
|       ||       ||  | |  |  |       ||       |  |   |   __|  | 
|       ||       ||  |_|  |  |       ||       |  |   |  |_____| 
|   _   ||   _   ||       |  |   _   || ||_|| |  |   |    __    
|__| |__||__| |__||_______|  |__| |__||_|   |_|  |___|   |__|   
```

---

![alien](../Assets/alien_2.png)

- Game loop, state, and parts
- Natural language processing
- Parser and dispatcher
- Items and behaviors
- How to build a game...
- What's next?

```Gold
 _______  _______  _______  __    _  ______   _______ 
|   _   ||       ||       ||  |  | ||      | |   _   |
|  |_|  ||    ___||    ___||   |_| ||  _    ||  |_|  |
|       ||   | __ |   |___ |       || | |   ||       |
|       ||   ||  ||    ___||  _    || |_|   ||       |
|   _   ||   |_| ||   |___ | | |   ||       ||   _   |
|__| |__||_______||_______||_|  |__||______| |__| |__|
```

---

```SpringGreen

Await Input ->
    Parse Input ->
            Dispatch Action ->
                Update Game State ->
                    Render Output ->
                        Loop


```

- All text outputs treated as markdown
- Custom markdown console renderer, based on CommonMark.NET
- Image support (each pixel = 1 character)
- Pygmentize used for code highlighting...

```SkyBlue


 _______  _______  __   __  _______    ___      _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |   |    |       ||       ||       |
|    ___||  |_|  ||       ||    ___|  |   |    |   _   ||   _   ||    _  |
|   | __ |       ||       ||   |___   |   |    |  | |  ||  | |  ||   |_| |
|   ||  ||       ||       ||    ___|  |   |___ |  |_|  ||  |_|  ||    ___|
|   |_| ||   _   || ||_|| ||   |___   |       ||       ||       ||   |    
|_______||__| |__||_|   |_||_______|  |_______||_______||_______||___|    
```

---

```SpringGreen

The game loop is a recursive function over the gamestate.


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

```SkyBlue

 _______  _______  __   __  _______    ___      _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |   |    |       ||       ||       |
|    ___||  |_|  ||       ||    ___|  |   |    |   _   ||   _   ||    _  |
|   | __ |       ||       ||   |___   |   |    |  | |  ||  | |  ||   |_| |
|   ||  ||       ||       ||    ___|  |   |___ |  |_|  ||  |_|  ||    ___|
|   |_| ||   _   || ||_|| ||   |___   |       ||       ||       ||   |    
|_______||__| |__||_|   |_||_______|  |_______||_______||_______||___|    
```

---

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

and Player = {
    Name: string
    Health: Health
    Experience: Experience
    Stats: Stats
    }

and Environment = {
    Id: EnvironmentId
    Name: string
    Description: string
    Exits: Exit list
    InventoryItems: InventoryItem list
    EnvironmentItems: EnvironmentItem list
    LightSource: InventoryItem option
    }

and World = {
    Time: System.DateTime
    Map: Environment []
    }

and Output =
    | Output of string list
    | DoNothing
    | ExitGame
    | Rollback
    | GameOver
```

```Lime
 _______  _______  __   __  _______    _______  _______  _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |       ||       ||   _   ||       ||       |
|    ___||  |_|  ||       ||    ___|  |  _____||_     _||  |_|  ||_     _||    ___|
|   | __ |       ||       ||   |___   | |_____   |   |  |       |  |   |  |   |___ 
|   ||  ||       ||       ||    ___|  |_____  |  |   |  |       |  |   |  |    ___|
|   |_| ||   _   || ||_|| ||   |___    _____| |  |   |  |   _   |  |   |  |   |___ 
|_______||__| |__||_|   |_||_______|  |_______|  |___|  |__| |__|  |___|  |_______|
```

---

```SpringGreen
Serializable


```

```fsharp
module IO =
    let saveGameState filename gamestate =
        let json = JsonConvert.SerializeObject(gamestate)
        System.IO.File.WriteAllText(filename, json)

    let loadGameState filename =
        let json = System.IO.File.ReadAllText(filename)
        JsonConvert.DeserializeObject<GameState>(json)
```

```Lime


 _______  _______  __   __  _______    _______  _______  _______  _______  _______ 
|       ||   _   ||  |_|  ||       |  |       ||       ||   _   ||       ||       |
|    ___||  |_|  ||       ||    ___|  |  _____||_     _||  |_|  ||_     _||    ___|
|   | __ |       ||       ||   |___   | |_____   |   |  |       |  |   |  |   |___ 
|   ||  ||       ||       ||    ___|  |_____  |  |   |  |       |  |   |  |    ___|
|   |_| ||   _   || ||_|| ||   |___    _____| |  |   |  |   _   |  |   |  |   |___ 
|_______||__| |__||_|   |_||_______|  |_______|  |___|  |__| |__|  |___|  |_______|
```

---

```SpringGreen
Game logic is written as game parts.  A game part is simply a function that manipulates the game state.

```

```fsharp
type GamePart = GameState -> GameState

```

```SpringGreen

```

- Inspired by Suave WebParts
- Pure functions
- Composable

```SkyBlue


 _______  _______  __   __  _______  _______  _______  ______    _______ 
|       ||   _   ||  |_|  ||       ||       ||   _   ||    _ |  |       |
|    ___||  |_|  ||       ||    ___||    _  ||  |_|  ||   | ||  |_     _|
|   | __ |       ||       ||   |___ |   |_| ||       ||   |_||_   |   |  
|   ||  ||       ||       ||    ___||    ___||       ||    __  |  |   |  
|   |_| ||   _   || ||_|| ||   |___ |   |    |   _   ||   |  | |  |   |  
|_______||__| |__||_|   |_||_______||___|    |__| |__||___|  |_|  |___|  
```

---

```SpringGreen
How to compose the "take {item}" GamePart

```

```fsharp

let takeItem item : GamePart =
    fun gamestate ->
        gamestate
        |> Environment.removeItemFromEnvironment item
        |> Inventory.addItem item
        |> World.updateWorldEnvironment
        |> Output.setOutput (Output (getSuccessOutputs item))

// Could also be written as ...
let takeItem item gamestate ->
    gamestate
    |> Environment.removeItemFromEnvironment item
    ...

```

```SkyBlue
 _______  _______  __   __  _______  _______  _______  ______    _______ 
|       ||   _   ||  |_|  ||       ||       ||   _   ||    _ |  |       |
|    ___||  |_|  ||       ||    ___||    _  ||  |_|  ||   | ||  |_     _|
|   | __ |       ||       ||   |___ |   |_| ||       ||   |_||_   |   |  
|   ||  ||       ||       ||    ___||    ___||       ||    __  |  |   |  
|   |_| ||   _   || ||_|| ||   |___ |   |    |   _   ||   |  | |  |   |  
|_______||__| |__||_|   |_||_______||___|    |__| |__||___|  |_|  |___|  
```

---

```SpringGreen
How might we interpret what the user wants to do in a natural way?

```

- "take key" - easy
- "take the rusty key in the corner" - doable
- "I'd like to take that rusty key over there in the corner if it pleases you, the almighty zeus" - ridiculous?

```SkyBlue


 __    _  _______  _______  __   __  ______    _______  ___        ___      _______  __    _  _______  __   __  _______  _______  _______ 
|  |  | ||   _   ||       ||  | |  ||    _ |  |   _   ||   |      |   |    |   _   ||  |  | ||       ||  | |  ||   _   ||       ||       |
|   |_| ||  |_|  ||_     _||  | |  ||   | ||  |  |_|  ||   |      |   |    |  |_|  ||   |_| ||    ___||  | |  ||  |_|  ||    ___||    ___|
|       ||       |  |   |  |  |_|  ||   |_||_ |       ||   |      |   |    |       ||       ||   | __ |  |_|  ||       ||   | __ |   |___ 
|  _    ||       |  |   |  |       ||    __  ||       ||   |___   |   |___ |       ||  _    ||   ||  ||       ||       ||   ||  ||    ___|
| | |   ||   _   |  |   |  |       ||   |  | ||   _   ||       |  |       ||   _   || | |   ||   |_| ||       ||   _   ||   |_| ||   |___ 
|_|  |__||__| |__|  |___|  |_______||___|  |_||__| |__||_______|  |_______||__| |__||_|  |__||_______||_______||__| |__||_______||_______|
```

---

```SpringGreen
Language Understanding (LUIS)


```

- Machine learning-based service from Microsoft <https://www.luis.ai>
- Allows applications to interpret natural language inputs!

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
How does LUIS work?


```

- Intents - how to determine what a user wants to do
- Entities - like variables, used to pass important information
- Utterances - phrases for intent or entity used to train the model
- Patterns - utterance templates with placeholders used to further improve the model

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
FsLamp Intents


```

- __Look__: "look around", "describe my surroundings", "what's around me?"
- __Move__: "walk west", "go north", "east"
- __Take__: "take lamp", "take key", "take horse"
- __Put__: "put letter in mailbox", "shove letter in mailbox"
- __SwitchOff__: "switch lamp off", "turn off the lamp"
- __SwitchOn__: "switch lamp on", "turn on the lamp"
- __Use__: "use typewriter", "open door with key"

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
FsLamp Entities


```

- __Direction__: "go {north}", "climb {up}"
- __Item__: "take {lamp}", "put {key} in the {mailbox}"
- __ExamineOperation__: "{read} the letter", "{smell} the coffee"
- __SwitchOperation__: "turn {on} the slide projector", "switch the lamp {off}"

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
FsLamp Patterns


```

- "put {Item:source} in {Item:target}" -> __PUT__
- "take {Item:target} from {Item:source}" -> __TAKE__
- "open {Item:target} with {Item:source}" -> __USE__

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
What does LUIS actually return?


```

```json
{
  "query": "turn slide projector on",
  "topScoringIntent": {
    "intent": "SwitchOn",
    "score": 0.6997941
  },
  "intents": [
    {
      "intent": "SwitchOn",
      "score": 0.6997941
    }
  ],
  "entities": [
    {
      "entity": "slide projector",
      "type": "Item",
      "startIndex": 5,
      "endIndex": 17,
      "score": 0.5267918
    },
    {
      "entity": "on",
      "type": "SwitchOperation",
      "startIndex": 19,
      "endIndex": 20,
      "score": 0.6275403
    }
  ]
}
```

```SkyBlue
 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
LUIS domain model

```

```fsharp
type LuisQuery = {
    Query: string
    TopScoringIntent: IntentResult
    Entities: EntityResult list
    }

and IntentResult = {
    Intent: string
    Score: float
    }

and EntityResult = {
    Entity: string
    Type: string
    StartIndex: int
    EndIndex: int
    Score: float
    Role: string
    }
```

```SkyBlue


 ___      __   __  ___   _______ 
|   |    |  | |  ||   | |       |
|   |    |  | |  ||   | |  _____|
|   |    |  |_|  ||   | | |_____ 
|   |___ |       ||   | |_____  |
|       ||       ||   |  _____| |
|_______||_______||___| |_______|
```

---

```SpringGreen
How do we use LUIS as a "command" parser?

```

```fsharp
type CommandParser = string -> Command option
```

```SpringGreen


```

- Given any string input
- Use LUIS to parse out the Intents and Entities
- Construct commands based on the results

```Yellow


 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

---

```SpringGreen
What are these "commands" you speak of?

```

```fsharp
type Command =
    | Move of Direction
    | Look
    | LookIn of ItemName: string
    | Take of ItemName: string
    | TakeFrom of TargetName: string * ItemName: string
    | Drop of ItemName: string
    | Use of ItemName: string
    | UseWith of TargetName: string * ItemName: string
    | SwitchItemOn of ItemName: string
    | SwitchItemOff of ItemName: string
    | PutItem of TargetName: string * ItemName: string
    | SaveGame
    | Read of ItemName: string
    ...
```

```Yellow

 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

---

```SpringGreen
F# pattern matching used to match a LUIS result to a command


```

```fsharp
match query.TopScoringIntent with
...
| { Intent = "SwitchOn"}
| { Intent = "SwitchOff"} ->
    match query.Entities with
    | [e1; e2] when e1.Type = "Item" && e2.Type = "SwitchOperation" ->
        if e2.Entity = "on"
        then SwitchItemOn (e1.Entity) |> Some
        else SwitchItemOff (e1.Entity) |> Some
    ...
    | _ -> None
```

```Yellow


 _______  _______  ______    _______  _______  ______   
|       ||   _   ||    _ |  |       ||       ||    _ |  
|    _  ||  |_|  ||   | ||  |  _____||    ___||   | ||  
|   |_| ||       ||   |_||_ | |_____ |   |___ |   |_||_ 
|    ___||       ||    __  ||_____  ||    ___||    __  |
|   |    |   _   ||   |  | | _____| ||   |___ |   |  | |
|___|    |__| |__||___|  |_||_______||_______||___|  |_|

```

---

```SpringGreen
The dispatcher matches a command to an action and returns a new game state resulting from that action.


```

```fsharp
let dispatch command : GamePart =
    fun gamestate ->
        let action =
            match command with
            | NoCommand                         -> message "Nothing to do."
            | Move dir                          -> Explore.move dir
            | Look                              -> Explore.look
            | LookIn itemName                   -> Explore.lookIn itemName
            | Take itemName                     -> Explore.take itemName
            | TakeFrom (targetName, itemName)   -> Explore.takeFrom targetName itemName
            | Drop itemName                     -> Explore.drop itemName
            | Use itemName                      -> Explore.useItem itemName
            | UseWith (targetName, itemName)    -> Explore.useItemOn targetName itemName
            | SwitchItemOn itemName             -> Explore.switch itemName SwitchState.SwitchOn
            | SwitchItemOff itemName            -> Explore.switch itemName SwitchState.SwitchOff

            ...

        {gamestate with LastCommand = command } |> action
```

```Fuchsia


 ______   ___   _______  _______  _______  _______  _______  __   __  _______  ______   
|      | |   | |       ||       ||   _   ||       ||       ||  | |  ||       ||    _ |  
|  _    ||   | |  _____||    _  ||  |_|  ||_     _||       ||  |_|  ||    ___||   | ||  
| | |   ||   | | |_____ |   |_| ||       |  |   |  |       ||       ||   |___ |   |_||_ 
| |_|   ||   | |_____  ||    ___||       |  |   |  |      _||       ||    ___||    __  |
|       ||   |  _____| ||   |    |   _   |  |   |  |     |_ |   _   ||   |___ |   |  | |
|______| |___| |_______||___|    |__| |__|  |___|  |_______||__| |__||_______||___|  |_|
```

---

```SpringGreen
How do we build items in the game? Mailbox, key, lantern, letter, rock...


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

```SpringGreen

Benefits

```

- F# Record Type instead of OOP inhertence
- Optional properties (health, stats, switching) consumed by different items
- Ensures everything is serializable

```SpringGreen

Drawbacks

```

- As new items are created, new properties will likely be added too
- Fine for now, will address this later...

```HotPink


 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

---

```SpringGreen

A basic item, the sun, used to provide light

```

```fsharp
let theSun = {
    Id = ItemId 1
    Name = "sun"
    Description = "is shining overhead."
    Health = None
    SwitchState = None
    Stats = None
    Contains = None
    Behaviors = [
        (Description "Ball of fire", ProvidesLight)
        ]
    }
```

```HotPink


 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

---

```SpringGreen

Helper functions used to simplify item creation.

```

```fsharp
let mailbox =
    createInventoryItem
        "mailbox" "propped up by a small stick"
        None
        None
        None
        (Some [letter;])
        [
            (Behaviors.putIn "shoved inside the tiny mailbox.");
            (Behaviors.takeOut "taken from the scrappy mailbox.")]

```

```HotPink


 ___   _______  _______  __   __  _______ 
|   | |       ||       ||  |_|  ||       |
|   | |_     _||    ___||       ||  _____|
|   |   |   |  |   |___ |       || |_____ 
|   |   |   |  |    ___||       ||_____  |
|   |   |   |  |   |___ | ||_|| | _____| |
|___|   |___|  |_______||_|   |_||_______|
```

---

```SpringGreen

How do items interact with the game?

```

- Game behaviors give the player the ability to interact with items in the game
- Each behavior is assigned an Id and stored in a runtime cache
- Behavior Id's get persisted in GameState as part of an item

```SpringGreen

```

```fsharp
type UpdateGameStateBehavior =
    (ItemUse * InventoryItem * GameState) -> Result<GameState, UpdateGameStateFailure>

type UpdateItemBehavior =
    (ItemUse * InventoryItem) -> Result<InventoryItem, UpdateItemFailure>
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

---

```SpringGreen
Behaviors are saved to a cache at runtime


```

```fsharp
let mutable private gameStateBehaviorCache : Map<(Description * ItemUse), UpdateGameStateBehavior> =
    Map.empty

let addGameStateBehavior id b =
    gameStateBehaviorCache <- gameStateBehaviorCache.Add(id, b)
    id

let findGameStateBehavior id =
    gameStateBehaviorCache.TryFind id
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

---

```SpringGreen
Define behaviors


```

```fsharp
let updateHealthBehavior f : UpdateItemBehavior =
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
        (updateHealthBehavior (fun (Health (life, total)) -> Health(life - amount, total)))
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

---

```SpringGreen
Add the behaviors to items


```

```fsharp
let lantern =
    createInventoryItem
        "lantern" "with a full battery"
        (Some (Health(150, 150)))
        (Some Items.SwitchOff)
        None
        None
        [
            Behaviors.loseBattery "Battery Life" 1; // per turn
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

```SkyBlue


 _______  _______  __   __  _______  __   __  ___   _______  ______    _______ 
|  _    ||       ||  | |  ||   _   ||  | |  ||   | |       ||    _ |  |       |
| |_|   ||    ___||  |_|  ||  |_|  ||  |_|  ||   | |   _   ||   | ||  |  _____|
|       ||   |___ |       ||       ||       ||   | |  | |  ||   |_||_ | |_____ 
|  _   | |    ___||       ||       ||       ||   | |  |_|  ||    __  ||_____  |
| |_|   ||   |___ |   _   ||   _   | |     | |   | |       ||   |  | | _____| |
|_______||_______||__| |__||__| |__|  |___|  |___| |_______||___|  |_||_______|
```

---

```SpringGreen
Add items to Environments


```

```fsharp
let origin =
    (Environment.create 1 "Origin"                               // name
            (Utility.readTextAsset "1_Intro.md")                 // description
            [Exit.create 1 2 Open North (Steps 2) "Creaky door"] // exits
            [keyItem (ExitId 5); lanternItem; mailbox;]          // items
            []                                                   // environment stuff
            (Some ambientLight)                                  // light source
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

---

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

```SpringGreen
NPC conversations


```

- Use LUIS to simulate an open converstation with an NPC
- Unlock parts of the game through conversation
- Buy and sell items

```Gold


 _     _  __   __  _______  _______  __   _______      __    _  _______  __   __  _______  ______  
| | _ | ||  | |  ||   _   ||       ||  | |       |    |  |  | ||       ||  |_|  ||       ||      | 
| || || ||  |_|  ||  |_|  ||_     _||__| |  _____|    |   |_| ||    ___||       ||_     _||___   | 
|       ||       ||       |  |   |       | |_____     |       ||   |___ |       |  |   |    __|  | 
|       ||       ||       |  |   |       |_____  |    |  _    ||    ___| |     |   |   |   |_____| 
|   _   ||   _   ||   _   |  |   |        _____| |    | | |   ||   |___ |   _   |  |   |     __    
|__| |__||__| |__||__| |__|  |___|       |_______|    |_|  |__||_______||__| |__|  |___|    |__|   
```

---

```SpringGreen
How do you actually compose a game then?


```

```Gold

 _______  __   __  _______  ______    _______    ______   _______  __   __  _______ 
|       ||  | |  ||       ||    _ |  |       |  |      | |       ||  |_|  ||       |
|  _____||  |_|  ||   _   ||   | ||  |_     _|  |  _    ||    ___||       ||   _   |
| |_____ |       ||  | |  ||   |_||_   |   |    | | |   ||   |___ |       ||  | |  |
|_____  ||       ||  |_|  ||    __  |  |   |    | |_|   ||    ___||       ||  |_|  |
 _____| ||   _   ||       ||   |  | |  |   |    |       ||   |___ | ||_|| ||       |
|_______||__| |__||_______||___|  |_|  |___|    |______| |_______||_|   |_||_______|

```

---

![ufo](../Assets/UFO_1.png)

Check out FsLamp on Github <https://github.com/ilmaestro/FsLamp>

```Firebrick


 _______  __   __  _______  __    _  ___   _    __   __  _______  __   __ 
|       ||  | |  ||   _   ||  |  | ||   | | |  |  | |  ||       ||  | |  |
|_     _||  |_|  ||  |_|  ||   |_| ||   |_| |  |  |_|  ||   _   ||  | |  |
  |   |  |       ||       ||       ||      _|  |       ||  | |  ||  |_|  |
  |   |  |       ||       ||  _    ||     |_   |_     _||  |_|  ||       |
  |   |  |   _   ||   _   || | |   ||    _  |    |   |  |       ||       |
  |___|  |__| |__||__| |__||_|  |__||___| |_|    |___|  |_______||_______|
```
