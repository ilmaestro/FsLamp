module FsLamp.Game.Map
open FsLamp.Core.Primitives
open FsLamp.Core.Domain
open FsLamp.Core
open FsLamp.Game.Items
open FsLamp.Game.Characters
open System

let defaultMap (renderer: FsLamp.Core.IRenderer) =
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
            [Encounter.create "__Green Slime__ appears and is ready to attack you!" [greenSlime]]
            (Some ambientLight);
        );
        (Environment.create 3 "Long Hallway, North End"
            (Utility.readTextAsset "northhall.md")
            [ 
                Exit.create 4 2 Open South (Steps 6) "hallway";
                Exit.create 5 4 Locked East (Steps 6) "door with no features"]
            []
            []
            None
        );
        (Environment.create 4 "Office"
            "You see the remnants of an old dusty office. Clearly this place hasn't been used in years. Except for an old typewriter on the desk is missing most of its keys, the room is completely empty."
            [ Exit.create 6 3 Open West (Steps 6) "door with no features"; Exit.create 7 5 Hidden Down (Steps 2) "secret passage"]
            [typewriter]
            [Encounter.create "A __grue__ jumps out from behind the desk and gets ready to attack you!" [grue]]
            (Some ambientLight)
        );
        (Environment.create 5 "Secret Passage"
            (Utility.readTextAsset "secretpassage.md")
            [ Exit.create 7 4 Open Up (Steps 2) "Secret Entrance"; Exit.create 8 6 Hidden East (Steps 10) "dark passage towards the footsteps"]
            [rock]
            []
            (Some ambientLight)
        );
        (Environment.create 6 "Dark Passage"
            """Is it really a good idea to go chasing after such a terrible, unknown, thing? Probably not, but that hasn't stopped you so far."""
            [ Exit.create 9 5 Open West (Steps 10) "Secret Passage"; Exit.create 10 7 Open Down (Steps 2) "Hole in the ground"]
            [gold]
            []
            (Some ambientLight)
        );
        (Environment.create 7 "Dark Hole"
            "You've discovered the pit to hell! Now what?"
            [ Exit.create 11 8 Open Down (Steps 1000) "Rabbit Hole" ]
            [towel]
            []
            (Some ambientLight));
        (Environment.create 8 "The Meetup"
            (Utility.readTextAsset "meetup_1.md")
            []
            [slideProjector renderer] //[agendaDoc; gameLoopDoc; luisDoc; ]
            []
            (Some theSun)
            )
    |]


