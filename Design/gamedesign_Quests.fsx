#load "gamedesign_PlayerState.fsx"

open PlayerState

(*
system for keeping track of RPG quests
*)
let has f xs =
    xs |> List.tryFind f |> Option.isSome
let hasAction action =
    has (fun a -> a = action)
let hasInventory inv player = 
    has (fun i -> i = inv) player.Inventory
let hasTaskInState taskType taskState quest =
    has (fun t -> t.TaskType = taskType && t.TaskState = taskState ) quest.QuestTasks

let updateQuestState questState questTasks =
    match questState with
    | Active ->
        if questTasks |> List.forall (fun task -> task.TaskState = Complete) then Succeeded
        else Active
    | state -> state

let updateQuest quest actions (player: Player) =
    if quest.QuestState = Active then
        let updatedTasks = 
            quest.QuestTasks
            |> List.map (fun task -> 
                { task with 
                    TaskState = if task.TaskState <> Complete then task.UpdateState (player, quest, actions) else task.TaskState
                    })
        { quest with 
            QuestTasks = updatedTasks
            QuestState = updatedTasks |> updateQuestState quest.QuestState
            }
    else quest

let updatePlayerQuests actions player =
    let updatedQuests = player.Quests |> List.map (fun quest -> updateQuest quest actions player)
    { player with
        Quests = updatedQuests
        }

let addQuest quest player =
    { player with 
        Quests = quest :: player.Quests
        }

let createQuest name state tasks =
    { Name = name; QuestState = state; QuestTasks = tasks; }
   
let addTask name taskType updater quest =
    { quest with 
        QuestTasks = {Name = name; TaskType = taskType; TaskState = Incomplete; UpdateState = updater} :: quest.QuestTasks 
        }

let fstPlayerQuest player = player.Quests |> List.head

let assertQuestState state quest =
    quest.QuestState = state

/////
// quest: pick up item, go to specific location, use item
/////

let bookOfTheDead = artifactMap.["BookOfDead"]
let reciteWords = Say "Clateu verata necktaou"
let approachPedestal = Approach (34, 42)

let questAwakeTheDead =
    createQuest "Awaken the dead" Active []
    |> addTask "Find book of the dead" (Find bookOfTheDead) (fun (player, _, _) -> 
                            if player |> hasInventory bookOfTheDead then Complete
                            else Incomplete)
    |> addTask "Approach pedestal in the old graveyard." approachPedestal (fun (player, quest, _) ->
                            if quest |> hasTaskInState (Perform reciteWords) Complete then Complete
                            elif player.MapPosition = (34, 42) then CompleteAtCurrentMoment
                            else Incomplete)
    |> addTask "Recite words from the book of the dead" (Perform reciteWords) (fun (_, quest, actions) ->
                            if (quest |> hasTaskInState (Find bookOfTheDead) Complete) 
                                && (quest |> hasTaskInState approachPedestal CompleteAtCurrentMoment)
                                && (actions |> hasAction reciteWords) then Complete
                            else Incomplete)

let runQuestTests () =
    let mutable joeWithQuests = joe |> addQuest questAwakeTheDead
    printfn "1. Should be active: %A" (joeWithQuests |> updatePlayerQuests [] |> fstPlayerQuest |> assertQuestState Active)

    joeWithQuests <- joeWithQuests |> addInventory bookOfTheDead |> updatePlayerQuests []
    printfn "%A" joeWithQuests
    printfn "2. Should be active: %A" (joeWithQuests |> fstPlayerQuest |> assertQuestState Active)

    joeWithQuests <- joeWithQuests |> setPosition (34, 42) |> updatePlayerQuests []
    printfn "%A" joeWithQuests
    printfn "3. Should be active: %A" (joeWithQuests  |> fstPlayerQuest |> assertQuestState Active)

    joeWithQuests <- joeWithQuests |> updatePlayerQuests [reciteWords]
    printfn "%A" joeWithQuests
    printfn "3. Should have succeeded: %A" (joeWithQuests |> fstPlayerQuest |> assertQuestState Succeeded)
