module ConsoleService
open SkiaSharp
open CommonMark.Syntax
open System.Drawing

let private esc = string (char 0x1B)
let private csi = esc + "["
let printsequencef f = Printf.kprintf (fun s -> System.Console.Write(esc + s)) f
let printcsif f = Printf.kprintf (fun s -> System.Console.Write(csi + s)) f

let resetToInitialState() = printsequencef "c"
let cursorUp (rows: int) = printcsif "%iA" rows
let cursorDown (rows: int) = printcsif "%iB" rows
let cursorForward (columns: int) = printcsif "%iC" columns
let cursorBack (columns: int) = printcsif "%iD" columns
let cursorNextLine(lines: int) = printcsif "%iE" lines
let cursorPreviousLine (lines: int) = printcsif "%iF" lines
let setCursorPos (row: int) (column: int) = printcsif "%i;%iH" row column
let clearScreen () = printcsif "2J"
let clearLine () = printcsif "2K"
let insertLine (lines: int) = printcsif "%iL" lines
let deleteLine (lines: int) = printcsif "%iM" lines
let scrollUp (lines: int) = printcsif "%iS" lines
let scrollDown (lines: int) = printcsif "%iT" lines
let selectGraphicRendition (gr: int list) = printcsif "%sm" (System.String.Join(";", gr |> Seq.map string))
let setForeground i = selectGraphicRendition([30 + i])
let setBackground i = selectGraphicRendition([40 + i])
let resetColor() = selectGraphicRendition [0]
let highIntensity() = selectGraphicRendition [1]
let lowIntensity() = selectGraphicRendition [2]
let normalIntensity() = selectGraphicRendition [22]
let setExtendedForeground i = selectGraphicRendition [38; 5; i]
let setExtendedBackground i = selectGraphicRendition [48; 5; i]
let setForegroundRgb r g b = selectGraphicRendition [38; 2; r; g; b]
let setBackgroundRgb r g b = selectGraphicRendition [48; 2; r; g; b]
let saveCursorPos () = printcsif "s"
let restoreCursorPos () = printcsif "u"
let useAlternateScreenBuffer () = printcsif "?1049h"
let useMainScreenBuffer () = printcsif "?1049l"


let showBasicColors() =
    for i in 0..7 do
        setBackground i
        printf "  "
    resetColor()
    printfn ""

let show256Colors() =
    for i in 0..15 do
        setExtendedBackground i
        printf "  "
        if i = 7 then
            resetColor()
            printfn ""
    resetColor()
    printfn ""

    let extendedBlock i =
        for j in i..i+5 do
            setExtendedBackground j
            printf "  "

    for row in 0..5 do
        for b in 0..5 do
            extendedBlock (16 + 36*b + row*6)
            resetColor()
            printf " "
        printfn ""

let showBitmap path =
    use input = System.IO.File.OpenRead(path)
    use inputStream = new SKManagedStream(input)
    use bitmap = SKBitmap.Decode(inputStream)
    for y in 0..bitmap.Height-1 do
        for x in 0..bitmap.Width-1 do
            let px = bitmap.GetPixel(x, y)
            if (int px.Alpha) = 0 then
                setBackgroundRgb 0 0 0
            else
                setBackgroundRgb (int px.Red) (int px.Green) (int px.Blue)
            printf "  "
        resetColor()
        printfn ""

module Markdown =
    open System
    open CommonMark

    let rec printInline (il: Inline) =
        let next() =
            printInline il.FirstChild
            printInline il.NextSibling

        if il = null then ()
        else
            match il.Tag with
            | InlineTag.LineBreak ->
                printfn ""
                printfn ""
                next()
            | InlineTag.SoftBreak ->
                printfn ""
                next()
            | InlineTag.String ->
                printf "%s" il.LiteralContent
                next()
            | InlineTag.Strong
            | InlineTag.Emphasis ->
                highIntensity()
                next()
                resetColor()
                printf ""
            | InlineTag.Image ->
                showBitmap (il.TargetUrl)
                // next()
            | _ ->
                next()

    open System.Diagnostics



    let pygmentize lexer (contents: string) =
        let psi = new ProcessStartInfo( "pygmentize" );
        // let psi = new ProcessStartInfo( "cmd.exe" ); // windows fix.
        let mutable command = null :> Process
        psi.RedirectStandardInput <- true
        psi.RedirectStandardOutput <- true
        psi.StandardOutputEncoding <- System.Text.Encoding.UTF8
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- false
        psi.CreateNoWindow <- true
        psi.Arguments <- sprintf "-l %s -O style=colorful,linenos=1" lexer
        // psi.Arguments <- sprintf "/c chcp 65001 >NUL && pygmentize -l %s" lexer

        try
            
            command <- Process.Start(psi)

            use inputWriter = command.StandardInput
            inputWriter.AutoFlush <- true
            inputWriter.Write (contents)
            inputWriter.Close()

            let output = command.StandardOutput.ReadToEnd()
            command.WaitForExit()
            output
        finally
            if not <| isNull command then command.Close()

    let rec printBlock (block: Block) =
        let next() =
            printBlock block.FirstChild
            printBlock block.NextSibling

        if block = null then ()
        else
            match block.Tag with
            | BlockTag.Document -> 
                next()
            | BlockTag.Paragraph ->
                printInline block.InlineContent
                printfn ""
                next()
            | BlockTag.AtxHeading
            | BlockTag.SetextHeading ->
                setBackground (int block.Heading.Level)
                printInline block.InlineContent
                resetColor()
                printfn ""
                next()
            | BlockTag.List ->
                next()
            | BlockTag.ListItem ->
                printf " * "
                next()
            | BlockTag.ThematicBreak ->
                printfn ""
                printfn ""
                next()
            | BlockTag.FencedCode ->
                match block.FencedCodeData.Info with
                | lang when lang = "fsharp" || lang = "json"->
                    // run the data through pygmentize
                    printf "%s" (pygmentize lang (block.StringContent.ToString()))
                    printfn ""
                
                | info ->
                    let color = Color.FromName(info)
                    setForegroundRgb (int color.R) (int color.G) (int color.B)
                    printf "%s" (block.StringContent.ToString())
                    resetColor()
                    printfn ""
                next()
            | _ ->
                printInline block.InlineContent
                next()

    let renderSomething (input: string) =
        let settings = CommonMarkSettings.Default.Clone()
        use src = (new IO.StringReader(input)) :> IO.TextReader
        CommonMarkConverter.Parse(src, settings)
        |> printBlock


module PageView =

    let renderHeader text =
        saveCursorPos ()
        setCursorPos 2 0
        setBackground 2
        setForeground 0
        clearLine ()
        printf " %s" text
        resetColor ()
        printfn ""
        restoreCursorPos ()

    let drawScreen log room health exp time =
        Markdown.renderSomething log
        let header = sprintf "%s \t\t\t Health: %s \t\t\t Experience: %i \t\t\t %s" room health exp time
        renderHeader header
