module WindowsConsole
#nowarn "9"

open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop

let private INVALID_HANDLE_VALUE = nativeint -1
let private STD_OUTPUT_HANDLE = -11
let private ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004

[<DllImport("Kernel32")>]
extern void* private GetStdHandle(int nStdHandle)

[<DllImport("Kernel32")>]
extern bool private GetConsoleMode(void* hConsoleHandle, int* lpMode)

[<DllImport("Kernel32")>]
extern bool private SetConsoleMode(void* hConsoleHandle, int lpMode)

let enableVTMode() =
    let handle = GetStdHandle(STD_OUTPUT_HANDLE)
    if handle <> INVALID_HANDLE_VALUE then
        let mode = NativePtr.stackalloc<int> 1
        if GetConsoleMode(handle, mode) then
            let value = NativePtr.read mode
            let value = value ||| ENABLE_VIRTUAL_TERMINAL_PROCESSING
            SetConsoleMode(handle, value)
        else
            printfn "no get"
            false
    else
        printfn "no handle"
        false