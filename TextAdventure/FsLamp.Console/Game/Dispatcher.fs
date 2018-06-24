module Dispatcher
open Domain
open GameState
open Actions
open Items

let getSaveDataFilename = 
    "./SaveData/GameSave.json"

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