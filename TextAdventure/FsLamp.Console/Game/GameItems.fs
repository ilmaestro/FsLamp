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
            Behaviors.takeItem "You pickup the small, crusty key." true]

let typewriter =
    createBasicItem "typewriter" "collecting dust"
        [Behaviors.openSecretPassage "As you press down hard on one of the keys. The air begins to move around you. Suddenly, the desk under the typewriter shifts to reveal a secret passage with steps leading down almost immediately into darkness." (ExitId 7);
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
        (Some (Health(150, 150)))
        (Some Items.SwitchOff)
        None
        None
        [
            Behaviors.loseBattery "Batter Life" 1;
            Behaviors.batteryWarnings "Battery Warning"
                [
                    (0,0, "Lantern's batteries are dead.");
                    (10,10, "Lantern is getting extremely dim.");
                    (20,20, "Lantern is getting dim.");]
            Behaviors.takeItem "You pick up the lantern." true;
            Behaviors.turnOnOff "You turn the switch.";
            (Description "Light", ProvidesLight);
        ]

let slideProjector =
    createInventoryItem "slide projector"
        "presentation.md"
        None
        (Some Items.SwitchOff)
        None
        None
        [
            Behaviors.slidesOnOff ".... well that was interesting. Thanks!"
        ]
let gold =
    createBasicItem "Gold" "that probably fell out of someones pocket" [
        (Behaviors.takeItem "GOOOLD!" true;)
    ]

let letter =
    createBasicItem "letter" "" [
        (Description (Utility.readTextAsset "1_Intro_Letter.md"), Readable);
        Behaviors.takeItem "You pick up the crumpled letter." true;
        ]

let theWub =
    createBasicItem "wub" "" [
        Behaviors.theWubOutput;
        Behaviors.takeItem "You pick up the wub." true;
    ]

let mailbox =
    createInventoryItem
        "mailbox" "propped up by a small stick"
        None
        None
        None
        (Some [letter;])
        [
            (Behaviors.putIn "shoved inside the tiny mailbox.");
            (Behaviors.takeOut "taken from the scrappy mailbox.")]

// let agendaDoc =
//     createBasicItem "agenda" "" [
//         (Description (Utility.readTextAsset "meetup_agenda.md"), Readable);
//         ]

// let gameLoopDoc =
//     createBasicItem "game loop" "" [
//         (Description (Utility.readTextAsset "meetup_gameloop.md"), Readable);
//         ]

// let luisDoc =
//     createBasicItem "luis" "" [
//         (Description (Utility.readTextAsset "meetup_luis.md"), Readable);
//         ]