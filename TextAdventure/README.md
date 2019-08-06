# Text Adventure

## Dependencies

- CommonMark.NET
- Pygments (uses the pygmentize CLI) (pip install pygments)
- SkiaSharp

### Terminal Support

FsLamp requires a terminal that supports TrueColor.  See list of terminals at this link: <https://gist.github.com/XVilka/8346728>

## Game Loop

Input -> Parser -> Command -> Dispatcher -> Output

### Dispatcher

Dispatcher is a function that takes a command calls the appropriate action, returning a Result.

### GamePart

A foundational combinator, a GamePart is something that takes a gamestate and returns a new gamestate.

```fsharp
type GamePart = GameState -> GameState
```

### Items

Optional State Data

- Health
- On/Off
- Stats
- Contained items

Uses

- OpenExit
- UseOnExit
- PutOn
- PutIn
- TakeFrom
- AttackWith
- TurnOnOff
- Contains
- ApplyStats
- LoseLifeOnUpdate
- LogOutputs
- ProvidesLight
- CanTake

## Encounters

An encounter is a separate game loop in which the player engages with a monster.  The gamstate should keep track whether the player is in an encounter or not.

player actions (initial attempt)

- attack: automatically wins
- run: always run away

2nd attempt - must keep track of encounter state, in order to win the encounter, all monsters must not be alive.

- attack: does damage against monsters
- run: may succeed or fail

## Game Objects

What's the best way to seperate the data from the behaviors. In order to be able to serialize and persist the entire gamestate, any logic must be kept seperate from the data.  Objects can have state and behavior, but its' the state that can be persisted. So what's the best way to match the two together.

### Behaviors

Behaviors are essentially pieces of game logic that can be assigned to game objects through a BehaviorId. Behaviors are kept in a runtime cache and are used in the gameloop.  Update behaviors are called just before the main action takes place, Output behaviors occur just before the game displays output.

## Current task list

- [x] items can be taken
- [x] turn lantern on/off
- [x] some environments have no light, unless lantern is turned on
- [x] containers
  - put/take items from them
- [ ] status bar: current environment, player health, time
- [ ] separate Uses for turnOn and turnOff
- [ ] NPC conversations
  - buy/sell
  - unlock parts of the story