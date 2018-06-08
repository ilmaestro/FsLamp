# Text Adventure

## Steps

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
