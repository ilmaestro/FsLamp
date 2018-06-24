```FloralWhite

 ▄  █ ▄███▄   █     █ ▄▄  
█   █ █▀   ▀  █     █   █ 
██▀▀█ ██▄▄    █     █▀▀▀  
█   █ █▄   ▄▀ ███▄  █     
   █  ▀███▀       ▀  █    
  ▀                   ▀   
                          
```

*Basic commands*:

```LightGray
status                        - get the current player status
wait {seconds}                - wait for specified number of seconds
move {north|south|east|west}  - move in specified direction
help                          - show help
look                          - see what's around
take {item}                   - take things
use {item}                    - try to use the item
exit                          - exit the game
```

*Combat commands*:

```Firebrick
attack                        - attack!
run                           - run away!
```

```fsharp
let useDescribe itemName itemUse : GamePart =
    fun gamestate ->
        let itemOption = tryFindItemFromGame itemName gamestate
        let description = 
            maybe {
                let! item' = itemOption
                let! (desc, _) = item' |> tryFindItemUse itemUse
                return desc
            }
        match itemOption, description with
        | Some _, Some (Description desc) ->
            gamestate
            |> Output.setOutput (Output [desc])
        | Some _, None ->
            gamestate 
            |> Output.setOutput (Output [sprintf "%s doesn't appear to have that function." itemName])
        | None, _ ->
            gamestate
            |> Output.setOutput (Output [sprintf "Couldn't find %s." itemName])
```