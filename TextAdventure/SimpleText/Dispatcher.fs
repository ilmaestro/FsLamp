module Dispatcher
open Domain
open GameState
open Actions

let getSaveDataFilename = 
    "./SaveData/GameSave.json"

let dispatch command : GamePart =
    fun gamestate ->
        let action = 
            match command with
            // open explore
            | NoCommand     -> message "Nothing to do."
            | Status        -> status
            | Wait ts       -> Explore.wait ts
            | Exit          -> Explore.exitGame
            | Help          -> Explore.help
            | Move dir      -> Explore.move dir
            | Look          -> Explore.look
            | Undo          -> Explore.undo
            | Take itemName -> Explore.take itemName
            | Drop itemName -> Explore.drop itemName
            | Use itemName  -> message "not supported at this moment" //Explore.useItem itemName
            | SaveGame      -> Explore.save getSaveDataFilename
            // main menu
            | NewGame       -> MainMenu.startGame
            | LoadGame      -> MainMenu.loadGame >> MainMenu.startGame
            // in encounter
            | Attack        -> InEncounter.attack
            | Run           -> InEncounter.run

        {gamestate with LastCommand = command } |> action