module Utility

let readFile folder file =
    System.IO.File.ReadAllText(System.IO.Path.Combine(folder, file))

let readAsset = readFile "../Assets"
let readTextAsset = readFile "../Assets/Text"