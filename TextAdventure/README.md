# Text Adventure

## Steps

1. create a simple bot
2. design game state machine
3. interact with bot to change state

game loop:
Input -> Parser -> Command -> Result -> Output

### dispatcher

dispatcher is takes a command and dispatches to the correct handler, returning a Result.

### Interactions

Goal: interact with non-inventory things

- "use handle": if there's something in the room that matches the name, call it's uses
