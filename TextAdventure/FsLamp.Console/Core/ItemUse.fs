module ItemUse
open Primitives
open Items
open GameState

type UpdateItemFailure = {
    Item: InventoryItem
    Message: string
}
type UpdateGameStateFailure = {
    GameState: GameState
    Message: string
}
type UpdateItemBehavior = ((ItemUse * InventoryItem) -> Result<InventoryItem,UpdateItemFailure>)
type UpdateGameStateBehavior = ((ItemUse * InventoryItem * GameState) -> Result<GameState,UpdateGameStateFailure>)

type ItemBehavior =
| UpdateItem of UpdateItemBehavior
| UpdateGameState of UpdateGameStateBehavior

type WhereIsItem =
| InInventory
| InEnvironment

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


let failItemUpdate message item =
    Error { Item = item; Message = message }

let failGameStateUpdate message gs =
    Error { GameState = gs; Message = message }

let inline (==) (a: ItemUse) (b: ItemUse) =
    match a,b with
    // special cases, ignore discriminator
    | OpenExit _, OpenExit _
    | UseOnExit _, UseOnExit _
    | PutOn _, PutOn _
    | PutIn _, PutIn _
    | TakeOut _, TakeOut _
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


// let tryFindItemWithUseFromInventory (itemName: string) itemUse gamestate =
//     gamestate.Inventory
//     |> List.tryFind (fun i ->
//         (i.Name.ToLower()) = itemName.ToLower())
//     |> Option.bind (tryFindItemUse itemUse)

let findItemByUse itemUse items =
    items |> List.tryFind (tryFindItemUse itemUse >> Option.isSome)

// let anyItemWithUse itemUse items =
//     items |> List.exists (tryFindItemUse itemUse >> Option.isSome)

let itemIsSwitchedOn (item: Items.InventoryItem) =
    match item.SwitchState with
    | Some Items.SwitchOn -> true
    | _ -> false
let tryFindByName (name: string) (list: Items.InventoryItem list) =
    list |> List.tryFind (fun i -> i.Name.ToLower() = name.ToLower())

let tryFindUpdate f itemUse item =
    item
    |> tryFindItemUse itemUse
    |> Option.bind f
    |> Option.map (fun update -> (item, itemUse, update))

let tryFindItemFromGame name gamestate =
    [gamestate.Inventory; gamestate.Environment.InventoryItems]
    |> List.map (tryFindByName name)
    |> List.choose id
    |> List.tryHead

let whereIsItem item gamestate =
    [(InInventory, gamestate.Inventory); (InEnvironment, gamestate.Environment.InventoryItems)]
    |> List.map (fun (where, list) -> list |> List.tryFind (fun i -> i = item) |> Option.map (fun _ -> where))
    |> List.choose id
    |> List.tryHead

let tryRemoveItemsFromGame items gamestate =
    let inventory' = gamestate.Inventory |> List.except items
    let environmentItems = gamestate.Environment.InventoryItems |> List.except items

    gamestate
    |> Inventory.setInventory inventory'
    |> Environment.setEnvironmentItems environmentItems  

let tryFindBehaviorFromUse itemUse =
    let itemUpdate = findItemUseBehavior itemUse |> Option.map UpdateItem
    let gameUpdate = findGameStateBehavior itemUse |> Option.map UpdateGameState
    [itemUpdate; gameUpdate]
    |> List.choose id
    |> List.tryHead

let tryFindOneOf uses item =
    uses
    |> List.map (fun itemuse -> item |> tryFindItemUse itemuse)
    |> List.choose id
    |> List.tryHead
    
module Defaults =
    // Empty / defaults
    let Open = OpenExit (ExitId 0)
    let UseOnExit = UseOnExit (ExitId 0)
    let PutOn = PutOn (None)
    let PutIn = PutIn (None)
    let TakeOut = TakeOut (None)
    let AttackWith = AttackWith (MonsterId 0)
    let CanTake = CanTake true
    let TurnOnOff = TurnOnOff SwitchOn

    let Useable = [Open; UseOnExit]