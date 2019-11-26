module FsLamp.Game.Items
open FsLamp.Core
open FsLamp.Core.Primitives
open FsLamp.Core.Items
open FsLamp.Game

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
        [Behaviors.openSecretPassage (Utility.readTextAsset "typewriter.md") (ExitId 7);
            Behaviors.takeItem "After several attempts of trying to pick up the typewriter, you realize you don't actually want to carry this thing around." false]

let rock =
    createBasicItem "rock" "just lying around"
        [Behaviors.openSecretPassage "You throw the rock directly at the voice and hear a terrible scream.  Moments later you can hear footsteps running to the east away from you." (ExitId 8);]

let towel =
    createBasicItem "towel" "somehow hanging beside you"
        [Behaviors.takeItem "A towel, [The Hitchhiker's Guide to the Galaxy] says, is about the most massively useful thing an interstellar hitchhiker can have. Partly it has great practical value. You can wrap it around you for warmth as you bound across... ... ... just don't forget to bring a towel." true]

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
            Behaviors.loseBattery "Battery Life" 1;
            Behaviors.batteryWarnings "Battery Warning"
                [
                    (0,0, "Lantern's batteries are dead.");
                    (10,10, "Lantern is getting extremely dim.");
                    (20,20, "Lantern is getting dim.");]
            Behaviors.takeItem "You pick up the lantern." true;
            Behaviors.turnOnOff "You turn the switch.";
            (Description "Light", ProvidesLight);
        ]

let slideProjector renderer =
    createInventoryItem "slide projector"
        "presentation.md"
        None
        (Some Items.SwitchOff)
        None
        None
        [
            Behaviors.slidesOnOff ".... well that was interesting. Thanks!" renderer
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
