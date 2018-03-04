module Parser

open FSharp.Data

type Parser(url) = 
    let parseUrl = string url
    let results = HtmlDocument.Load("http://www.aleksandragranos.pl/")

    let links = 
        results.Descendants ["a"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("href")
               |> Option.map (fun a -> x.InnerText(), a.Value())
        )

    let images = 
        results.Descendants ["img"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("src")
               |> Option.map (fun a -> a.Value())
        )

    let scripts = 
        results.Descendants ["script"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("src")
               |> Option.map (fun a -> a.Value())
        )    

    member _this.SearchResults =
        links
        |> Seq.filter (fun (name, url) -> 
                        name <> "Cached" && name <> "Similar" && url.StartsWith("/url?"))
        |> Seq.map (fun (name, url) -> name, url.Substring(0, url.IndexOf("&sa=")).Replace("/url?q=", ""))
        |> Seq.toArray



