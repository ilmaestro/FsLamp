module GameItems
open Primitives
open Items
open GameBehaviors

let createBasicItem name description behaviors =
    createInventoryItem name description None None None None behaviors

let theSun =
    createBasicItem "sun" "is shining overhead." [(Description "ball of fire", ProvidesLight)]

let ambientLight =
    createBasicItem "ambient light" "" [(Description "ambient light", ProvidesLight)]

// this key is used to open
let keyItem exitId = 
    createBasicItem
        "key" "laying in a pile of debris"
        [Behaviors.openExit "After a few minutes of getting the key to fit correctly, the lock releases and the door creakily opens." exitId;
            Behaviors.takeItem "You pickup a small, crusty key." true]

let typewriter =
    createBasicItem "typewriter" "collecting dust"
        [Behaviors.openSecretPassage "As you press down hard on one of the keys. The air begins to move around you. Suddenly, a secret passage opens up from within the wall." (ExitId 7);
            Behaviors.takeItem "After several attempts of trying to pick up the typewriter, you realize you don't actually want to carry this thing around." false]

let rock =
    createBasicItem "rock" "just lying around"
        [Behaviors.openSecretPassage "You throw the rock directly at the voice and hear a terrible scream.  Moments later you can hear footsteps running to the east away from you." (ExitId 8);]

// lantern is an item you can take that allows you to see in dark places.
// it can be turned on & off
// it consumes battery life when it's turned on
let lanternItem =
    createInventoryItem
        "lantern" "with a full battery"
        (Some (Health(15, 15)))
        (Some Items.SwitchOff)
        None
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
            (Description "Light", ProvidesLight);
        ]

let gold =
    createBasicItem "Gold" "that probably fell out of someones pocket" []

let mailbox =
    createInventoryItem
        "mailbox" "propped up by a small stick"
        None
        None
        None
        (Some [])
        [
            (Description "Holds 1 Item", Contains 1);
            (Behaviors.putIn "shoved inside the tiny mailbox");
            (Behaviors.takeOut "taken from the scrappy mailbox")]
