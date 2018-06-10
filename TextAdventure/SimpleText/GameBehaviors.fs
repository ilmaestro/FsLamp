module GameBehaviors

open Domain
open GameState

let private cache : Map<BehaviorId, GameBehavior<InventoryItem>> ref = ref Map.empty

let add b =
    let id = BehaviorId ((!cache).Count + 1)
    cache := (!cache).Add(id, b)
    id

let find id =
    (!cache).TryFind(id)

module Temporary =
    let decrementLifeOnUpdateBehavior = 
        UpdateBehavior (
            fun (item: InventoryItem) gs ->
                match item with
                | TemporaryItem (props, life) ->
                    let newItem = TemporaryItem (props, life - 1)
                    (newItem, gs)
                | _ ->
                    (item, gs))

    let rangedOutputBehavior (ranges: (int * int * string) list) =
        OutputBehavior (
            fun (item: InventoryItem) _ ->
                match item with
                | (TemporaryItem (_, life)) ->
                    match ranges |> List.tryFind (fun (min, max, _) -> min <= life && life <= max) with
                    | Some (_,_,output) -> [output]
                    | None -> []
                    // if life < 1 then ["Battery has run out."]
                    // else if life < 5 then ["Light is getting dim."]
                    // else []
                | _ -> []
        )

