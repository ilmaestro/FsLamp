# Revie Game Design

## Game Mechanics

Major mechanics in the game:
- Engagements
- Menus
- Dialogs
- Movement and Actions
- Quests
- Cut scenes
- Title scene

### Engagements
- Engaged: any time the player enters a scene to resolve a direct conflict the player becomes Engaged. This mostly includes fight scenes or scenes where player can interact with NPCs using some kind of menu.  The menu can be different depending on the engagement. This can also include dialog interactions.

- Non-Engaged: The majority of the time the player is non-engaged and free to wonder around the game.

### Menus
Menus are used by the player to perform tasks. There are a few types of menus used throughout the game:
- Title/Pause menu
- Non-engaged menu
- Engaged menu

#### Title/Pause menu
- Allows the player to start a new game, save an in-progress game, open an existing game, or delete an existing game.
- Initially opened from the title scene of the game when starting a game
- Can be opened at any time during gameplay (even during a battle?)
- When can you not save a game?
    - Game in-progress, but in an engagement
    - When theres no game in-progress

- Tasks:
    - New
    - Save
    - Open
    - Delete

#### Non-Engaged menu
- Allows the player to perform in-game tasks like: viewing stats, using items/skills, viewing quests, equiping
- Only opened when the player is not engaged with anything
- Menu
    - Status
        - Quest
        - Characters
    - Items
        - list of selectable items
        - list of Stats that might be affected by using items?
        - combine items?
        - Actions for items in context
            - Use item
                - on self?
                - on other?
            - Drop item
            - Give item
                - to who?
    - Skills
        - list of selectable skills
        - combine skills?
        - Actions for skill in context
            - Perform skill
                - on self?
                - on other?
    - Equipment
        - list of equipped 
        - list of stats that might be affected by equipping things?
        - combine equipment?
        - Actions for equipment in context
            - Equip (if not equipped)
            - Unequip (if equipped)
            - Drop
            - Give
                - to who?
    - Quests
        - list of quests
        - Actions for quest in context?
            - make notes
            - hints
            - start quest (if not in progress)
            - pause quest ( if this makes sense?)
            - give up
    - Commands
        - Quick command setup
            - List of commands
            - select a command for the Quick command (default)
        - Special commands
            - list of commands
            - combine commands?
            - create new command?
            - delete commands?
        - 

#### Engaged menus
- There are different types of engagements: Fights, Talking to NPCs, Cut scenes, etc...
- Fights will have the most complex menu, while others may not have menus at all
- Fight Menu (during players turn)
    - Last command (to save button presses)
    - Command
        - list of selectable commands
        - Select command to execute
    - Item
        - list of selectable items
        - Select item to use
            - on self?
            - on other?
    - Run
- Talking/Next menu (when talking to NPCs) 
    - ``show dialog text`` line by line
    - Next (use next to see next line of text)
    - End (when there's no more text)
- Talking/Question menu
    - ``show question `` line
    - ``show answers`` selections
    - Feed answer back into menu to get next question/answers
    - if no answers, use Talking/Next Menu
- Cut scene menu
    - wait for scene to reach stopping point
    - Next (if the player needs to press somethign to continue)
    - End (if the scene is over)

### Dialogs
- Dialogs are the containers for displaying interactable text on the screen.  
- Menus depend on dialogs in order to display text on the screen.
- Should be Bordered, Colored, and Animated 

### Movement and Actions
- Buttons
    - D-pad (4 directions)
    - Action button
    - Back/Cancel/Escape button
    - Pause button
- During non-engaged gameplay the character uses the d-pad to move
- During engaged gameplay the character navigates menus via the d-pad, action and back buttons
    - d-pad used to move the selection
    - action button used to invoke selection
    - back butotn used to cancel current selection or exit the menu completely


### Quests
- Quests are used to keep track of where the player is in the game
- Quests often have rewards for completion
- Quests are organized into a sequential list of tasks
- Tasks have triggers to complete them, which in turn update the progress of the quest
- When all tasks are completed in a quest, the Quest is completed


### Cut Scenes
- typically non-interactive, where the player is watching something happen in real-time
- may have stopping points where the player can interact or at least needs to hit the action button in order to continue the scene


### Title Scene
- After the game initializes, the title scene is displayed
- Displays the game title
- Should be colorful, animated
- Allows the player to access the title menu where a game can be started
- Secret button sequences?
    - show credits
    - game modifiers?