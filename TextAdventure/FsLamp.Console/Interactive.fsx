#I "../../packages/Newtonsoft.Json/lib/netstandard2.0"
#I "../../packages/NETStandard.Library/build/netstandard2.0/ref"
#I "../../packages/SkiaSharp/lib/netstandard1.3"
#I "../../packages/SkiaSharp/runtimes/osx/native"
#I "../../packages/CommonMark.NET/lib/netstandard1.0"
#r "netstandard"
#r "Newtonsoft.Json"
#r "SkiaSharp"
#r "CommonMark"

#load "Core/ConsoleService.fs"

open ConsoleService


showBasicColors()
show256Colors()

showBitmap "./TextAdventure/Assets/fslogo.bmp"

open System.Diagnostics

let test() =
    let processStart = 
        ProcessStartInfo(
            FileName = "cmd.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            Arguments = "/c chcp 65001 > null && pygmentize ./FsLamp.Console/Program.fs"
        )
    let p = Process.Start(processStart)
    let output = p.StandardOutput.ReadToEnd()
    p.WaitForExit()
    output
test()