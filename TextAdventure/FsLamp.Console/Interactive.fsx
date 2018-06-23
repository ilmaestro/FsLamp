#I "../../packages/Newtonsoft.Json/lib/netstandard2.0"
#I "../../packages/NETStandard.Library/build/netstandard2.0/ref"
#I "../../packages/SkiaSharp/lib/netstandard1.3"
#I "../../packages/SkiaSharp/runtimes/osx/native"
#r "netstandard"
#r "Newtonsoft.Json"
#r "SkiaSharp"

#load "Core/ConsoleService.fs"

open ConsoleService

showBasicColors()
show256Colors()

showBitmap "./TextAdventure/Assets/fslogo.bmp"

