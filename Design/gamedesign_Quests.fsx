#load "gamedesign_PlayerState.fsx"

(*
system for keeping track of RPG quests
*)
type Player = {
    id: System.Guid
    name: string
    location: int * int
    items: Item list
    actions: Action list
    quests: Quest list
}

and Item = 
| BookOfTheDead
| SwordOfBo
| AquariumOfSand

and Action = 
| Using of Item

// quest = series of tasks that must be completed by triggering specific events
and Quest = {
    name: string
    questState: QuestState
    questTasks: QuestTask list
}

and QuestState = 
| Inactive
| Active
| Succeeded
| Failed

and QuestTask = {
    taskType: TaskType
    name: string
    taskState: TaskState
    updateState: (Player * Quest) -> TaskState
}
and TaskType =
| FindBookOfDead
| ApproachPedestal
| ReciteWords

and TaskState =
| Incomplete
| Complete
| CompleteAtCurrentMoment

///
let hasAction action player =
    let rec find actions =
        match actions with
        | [] -> false
        | hd :: tail -> 
            if hd = action then true 
            else find tail
    find player.actions
   
let hasItem item player =
    let rec find items =
        match items with
        | [] -> false
        | hd :: tail -> 
            if hd = item then true 
            else find tail
    find player.items

let hasTaskInState taskType taskState quest =
    let rec find tasks =
        match tasks with
        | [] -> false
        | hd :: tail ->
            if hd.taskType = taskType then hd.taskState = taskState
            else find tail
    find quest.questTasks

let getQuestState questState questTasks =
    match questState with
    | Inactive -> Inactive
    | Active ->
        if questTasks |> List.forall (fun task -> task.taskState = Complete) then Succeeded
        else Active
    | state -> state

let updateQuest quest player =
    if quest.questState = Active then
        let updatedTasks = 
            quest.questTasks
            |> List.map (fun task -> 
                { task with 
                    taskState = if task.taskState <> Complete then task.updateState (player, quest) else task.taskState
                    })
                   
        { quest with 
            questTasks = updatedTasks
            questState = getQuestState quest.questState updatedTasks
            }
    else quest
    

let updatePlayer player =
    let updatedQuests = player.quests |> List.map (fun quest -> updateQuest quest player)
    { player with
        quests = updatedQuests
        }
    
/////
// quest: pick up item, go to specific location, use item
/////
let player = {
    id = (System.Guid.NewGuid())
    name = "Juan"
    location = (0, 0)
    items = []
    quests = []
    actions = []
}

let questAwakeTheDead = {
    name = "Awaken the dead"
    questState = Active
    questTasks = [ {
                        name = "Find book of the dead."
                        taskType = FindBookOfDead
                        taskState = Incomplete
                        updateState = fun (player, _) -> 
                            if player |> hasItem BookOfTheDead then Complete
                            else Incomplete
                    };
                    { 
                        name = "Approach pedestal in the old graveyard."
                        taskType = ApproachPedestal
                        taskState = Incomplete
                        updateState = fun (player, quest) ->
                            if quest |> hasTaskInState ReciteWords Complete then Complete
                            elif player.location = (34, 42) then CompleteAtCurrentMoment
                            else Incomplete
                    };
                    {
                        name = "Recite words from the book of the dead"
                        taskType = ReciteWords
                        taskState = Incomplete
                        updateState = fun (player, quest) ->
                            if (quest |> hasTaskInState FindBookOfDead Complete) 
                                && (quest |> hasTaskInState ApproachPedestal CompleteAtCurrentMoment)
                                && (player |> hasAction (Using(BookOfTheDead))) then Complete
                            else Incomplete
                    };
    ]
}

let playerID = System.Guid.NewGuid()
let nextPlayer = updatePlayer {
                                    id = playerID
                                    name = "Juan"
                                    location = (34, 42)
                                    items = [BookOfTheDead]
                                    quests = [questAwakeTheDead]
                                    actions = [Using(BookOfTheDead)]
                                }

//