module FSharpDataMiningDemo

open Parser
open System

[<EntryPoint>]
let main argv =
    printfn "%A" argv
    let parser = new Parser("http://www.aleksandragranos.pl/");
    printfn "%A" parser.SearchResults
    Console.ReadKey() |> ignore
    0 // return an integer exit code
