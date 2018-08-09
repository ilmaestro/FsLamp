# FsLamp Presentation

## overview

- Architecture
- LUIS
- GamePart
- Behaviors
- Making a game
- Where to from here?

## Architecture

### Game loop

- [Print to Screen]
- [Get Input from Player]
- [Parse input via LUIS]
- [Create command with inputs]
- [Dispatch command]
- [Handle Output]
- [Loop]

### Game state

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

### Dispatcher

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

## LUIS

### Basic Info

### Configuration

## GamePart

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

## Behaviors

Game behaviors are what give the player the ability to interact with components of the game.

