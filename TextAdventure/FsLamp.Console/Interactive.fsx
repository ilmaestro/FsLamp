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

