# Text Adventure

## first Steps

1. create a simple bot
2. design game state machine
3. interact with bot to change state

game loop:
Input -> Parser -> Command -> Result -> Output

### dispatcher

dispatcher is takes a command and dispatches to the correct handler, returning a Result.

### Inventory Items

Categories

- Inventory Item: something useful (has uses)
- Artifact: something of value and may be required to do something.
- Weapon: something used to attack with

Uses

- Unlock: changes a locked exit into an open exit.
- Unhide: changes a hidden exit into an open exit.
- Add Experience: adds experience points to player.
- Add Item: adds an item to players inventory.

### Environment Items

Categories

- Environment Item: something the player can interact with in the current environment
- Monster Encounter: something the player will encounter in the current environment
- NPC Encounter: something the player can talk to in the current environment

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

## Start to organize

- [x] items can be taken
- [x] turn lantern on/off
- [x] some environments have no light, unless lantern is turned on
- [ ] containers that open/close and allow you to put items inside them
- [ ] status bar: current environment, player health, time
- [ ] NPC conversations
  - buy/sell
  - unlock parts of the story