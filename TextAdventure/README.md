# Text Adventure

## Steps

1. create a simple bot
2. design game state machine
3. interact with bot to change state

game loop:
Input -> Parser -> Command -> Result -> Output

### dispatcher

dispatcher is takes a command and dispatches to the correct handler, returning a Result.

### Items

#### Uses

- Unlock: changes a locked exit into an open exit.
- Unhide: changes a hidden exit into an open exit.
- Add Experience: adds experience points to player.
- Add Item: adds an item to players inventory.