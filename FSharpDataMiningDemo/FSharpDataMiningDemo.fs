module FSharpDataMiningDemo

open Parser
open System

[<EntryPoint>]
let main argv =
    let arglist = argv |> List.ofSeq
    printfn "%A" arglist
    let url = List.exists ((=) "-url") arglist
    let urlIndex = if (url) then List.findIndex (fun elem -> elem = "-url") arglist else -1
    let urlName = if(urlIndex >= 0) then arglist.[urlIndex+1] else ""
    let writeToFile = List.exists ((=) "-file") arglist
    let fileIndex = if (writeToFile) then List.findIndex (fun elem -> elem = "-file") arglist else -1
    let filePath = if(fileIndex >= 0) then arglist.[fileIndex+1] else ""
    let writeToConsole = List.exists ((=) "-console") arglist
    let getLinks = List.exists ((=) "-a") arglist
    let getImages = List.exists ((=) "-img") arglist
    let getScripts = List.exists ((=) "-script") arglist
    let countWords = List.exists ((=) "-text") arglist
    let goDepth = List.exists ((=) "-depth") arglist
    let goDepthIndex = if (goDepth) then List.findIndex (fun elem -> elem = "-depth") arglist else -1
    let depthLevel = if(goDepthIndex >= 0) then arglist.[goDepthIndex+1] else ""

    let parser = new Parser(urlName, writeToConsole, writeToFile, filePath, depthLevel |> int);
    parser.SetUrlsToVisit
    //parser.PrintUrlsToVisit
    if getImages then parser.GetImages
    if getLinks then parser.GetLinks
    if getScripts then parser.GetScripts
    if countWords then parser.CountWords

    parser.CountCosinus
    //FSharpDataMiningDemo.exe -url "URL" -file 1.txt -console -depth 2
    

    parser.Dispose
    Console.ReadKey() |> ignore
    0 // return an integer exit code
