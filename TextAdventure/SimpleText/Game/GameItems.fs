module GameItems
open Primitives
open Items
open GameBehaviors

// this key is used to open
let keyItem = 
    createInventoryItem
        (ItemId 1)
        "key" "laying in a pile of debris"
        None // no health
        None
        None
        [Behaviors.openExit "After a few minutes of getting the key to fit correctly, the lock releases and the door creakily opens." (ExitId 5);
            Behaviors.takeItem "You pickup a small, crusty key." true]

let typewriter =
    createInventoryItem (ItemId 4) "typewriter" "collecting dust" None None None 
        [Behaviors.openSecretPassage "As you press down hard on one of the keys. The air begins to move around you. Suddenly, a secret passage opens up from within the wall." (ExitId 7);
            Behaviors.takeItem "After several attempts of trying to pick up the typewriter, you realize you don't actually want to carry this thing around." false]

let rock =
    createInventoryItem (ItemId 5) "rock" "just lying around" None None None
        [Behaviors.openSecretPassage "You throw the rock directly at the voice and hear a terrible scream.  Moments later you can hear footsteps running to the east away from you." (ExitId 8);]

// lantern is an item you can take that allows you to see in dark places.
// it can be turned on & off
// it consumes battery life when it's turned on
let lanternItem =
    createInventoryItem
        (ItemId 2) 
        "lantern" "with a full battery"
        (Some (Health(15, 15)))
        (Some Items.SwitchOff)
        None
        [
            Behaviors.loseBattery "Batter Life" 1;
            Behaviors.batteryWarnings "Battery Warning"
                [
                    (0,0, "Lantern's batteries are dead.");
                    (5,5, "Lantern is getting extremely dim.");
                    (10,10, "Lantern is getting dim.");]
            Behaviors.takeItem "You pick up the lantern" true;
            Behaviors.turnOnOff "Turns the light on and off";
        ]

let gold =
    createInventoryItem (ItemId 3) "Gold" "that probably fell out of someones pocket" None None None []
