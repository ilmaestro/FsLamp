module GameMap
open Primitives
open Domain
open Environment
open GameItems
open GameMonsters


let defaultMap () =
    [|
        (Environment.create 1 "Origin"
            (Utility.readTextAsset "1_Intro.md")
            [Exit.create 1 2 Open North (Steps 2) "Creaky door"]
            [keyItem (ExitId 5); lanternItem; mailbox;]
            []
            (Some ambientLight)
        );
        (Environment.create 2 "Long Hallway, South End"
            "The door opens into what appears to be a really long hallway continuing North. There's no light at the other end."
            [
                Exit.create 2 1 Open South (Steps 2) "Creaky door";
                Exit.create 3 3 Open North (Steps 6) "Dark hallway";]
            []
            [Encounter.create "Green Slime appears and is attacking you!" [greenSlime]]
            (Some ambientLight);
        );
        (Environment.create 3 "Long Hallway, North End"
            "It gets so dark you have to feel your way around. Thankfully there's nothing too dangerous in your path."
            [ 
                Exit.create 4 2 Open South (Steps 6) "The south end of the hallway";
                Exit.create 5 4 Locked East (Steps 6) "A door with no features, labeled 'Private'"]
            []
            []
            None
        );
        (Environment.create 4 "Office"
            "As the door opens, you begin to see the remnants of an old dusty office.  This place hasn't been used in years. An old typewriter on the desk is missing most of its keys."
            [ Exit.create 6 3 Open West (Steps 6) "Door with no features"; Exit.create 7 5 Hidden East (Steps 2) "Secret Passage"]
            [typewriter]
            [Encounter.create "A gruet jumps out from the darkness." [gruet]]
            (Some ambientLight)
        );
        (Environment.create 5 "Secret Passage"
            """The path leads downward with considerable gradient. Things turn cold as you hear a voice... 'stoi impul chani, mario.' Frozen, but unable to make out any figures ahead of you, you shout back 'Who's there?'
A few seconds pass, finally a response... 'die!'.  As you fall backward you stumble over a rock.            
            """
            [ Exit.create 7 4 Open West (Steps 2) "Secret entrance"; Exit.create 8 6 Hidden East (Steps 10) "Dark Passage towards the footsteps"]
            [rock]
            []
            (Some ambientLight)
        );
        (Environment.create 6 "Dark Passage"
            """Is it really a good idea to go chasing after such a terrible, unknown, thing? Probably not, but that hasn't stopped you so far."""
            [ Exit.create 9 5 Open West (Steps 10) "Secret Passage"]
            [gold]
            []
            (Some ambientLight)
        );
    |]
