module ItemUse
open Primitives
open Items
open GameState

type UpdateItemBehavior = ((ItemUse * InventoryItem) -> InventoryItem)
type UpdateGameStateBehavior = ((ItemUse * InventoryItem * GameState) -> GameState)

let private itemUseBehaviorCache : Map<(Description * ItemUse),UpdateItemBehavior> ref = ref Map.empty
let private gameStateBehaviorCache : Map<(Description * ItemUse),UpdateGameStateBehavior> ref = ref Map.empty

let addItemUseBehavior id b =
    itemUseBehaviorCache := (!itemUseBehaviorCache).Add(id, b)
    id

let addGameStateBehavior id b =
    gameStateBehaviorCache := (!gameStateBehaviorCache).Add(id, b)
    id

let findItemUseBehavior id =
    (!itemUseBehaviorCache).TryFind id

let findGameStateBehavior id =
    (!gameStateBehaviorCache).TryFind id


let inline (==) (a: ItemUse) (b: ItemUse) = 
    match a,b with
        // special cases, ignore discriminator
        | OpenExit _, OpenExit _
        | UseOnExit _, UseOnExit _
        | PutOn _, PutOn _
        | PutIn _, PutIn _
        | TakeFrom _, TakeFrom _
        | AttackWith _, AttackWith _
        | CanTake _, CanTake _
            -> true

        // all other cases
        | x,y 
            -> x = y

let tryFindItemUse (itemUse: ItemUse) item =
    item.Behaviors
    |> List.tryFind (fun (_, i) -> i == itemUse)

let itemHasUse (itemUse: ItemUse) item =
    item.Behaviors
    |> List.exists (fun (_, i) -> i == itemUse)


module Defaults =
    // Empty / defaults
    let Open = OpenExit (ExitId 0)
    let UseOnExit = UseOnExit (ExitId 0)
    let PutOn = PutOn (ItemId 0)
    let PutIn = PutIn (ItemId 0)
    let TakeFrom = TakeFrom (ItemId 0)
    let AttackWith = AttackWith (MonsterId 0)
    let CanTake = CanTake true