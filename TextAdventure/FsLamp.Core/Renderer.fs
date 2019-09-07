namespace FsLamp.Core

open FsLamp.Core.GameState

// renderer interface

type IRenderer =
    abstract member RenderGameState: GameState -> unit
    abstract member RenderMarkdown: string -> unit
    abstract member Clear: unit -> unit
    abstract member Init: unit -> unit
    abstract member Close: unit -> unit