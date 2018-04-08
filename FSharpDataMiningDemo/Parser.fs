module Parser

#if INTERACTIVE
#r @"..\packages\FSharp.Data\lib\net45\FSharp.Data.dll"
#endif
open FSharp.Data
open System.Text

type Parser(url, console, writeToFile, filePath) = 
    let parseUrl = string url
    let writeToConsole = console;
    let encoding = Encoding.GetEncoding("Windows-1250")
    let results = HtmlDocument.Load(parseUrl, encoding)

    let file = System.IO.File.CreateText(filePath) 


    let visitedUrls = []
    
    let links = 
        results.Descendants ["a"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("href")
               |> Option.map (fun a -> a.Value())
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
               
    let words = ResizeArray<string>()

    let rec readNode (node:HtmlNode) =
        for element in HtmlNodeExtensions.Elements(node) do
            let text = HtmlNodeExtensions.InnerText(element)
            // printf "%A\n" text
            let textWords = text.Split [|' '|]
            textWords |> Seq.iter (fun x -> words.Add x)
            readNode(element)

    member _this.GetImages =
        printf "\nIMAGES:\n"
        if writeToConsole then images |> Seq.iter (fun x -> printf "%A\n" x)
        if writeToFile then 
            file.WriteLine("")
            file.WriteLine("IMAGES:")
            images |> Seq.iter (fun x -> file.WriteLine(x))
        
    member _this.GetLinks =
        printf "\n\nLINKS: \n"
        if writeToConsole then links |> Seq.iter (fun x -> printf "%A\n" x)
        if writeToFile then 
            file.WriteLine("")
            file.WriteLine("nLINKS:")
            links |> Seq.iter (fun x -> file.WriteLine(x))
    member _this.GetScripts =
        printf "\n\nSCRIPTS: \n"
        if writeToConsole then scripts |> Seq.iter (fun x -> printf "%A\n" x)
        if writeToFile then 
            file.WriteLine("")
            file.WriteLine("SCRIPTS:")
            scripts |> Seq.iter (fun x -> file.WriteLine(x))

    member _this.CountWords =
        let body = results.Descendants ["body"]
        let node = body |> Seq.head
        readNode(node)

        if writeToFile then 
            words
            |> Seq.countBy (fun s -> s.Trim())
            |> Seq.sortBy (snd >> (~-))
            |> Seq.iter (fun x -> printf "%A\n" x)

        if writeToFile then
            file.WriteLine("")
            file.WriteLine("\nWORDS:")
            words
            |> Seq.countBy (fun s -> s.Trim())
            |> Seq.sortBy (snd >> (~-))
            |> Seq.iter (fun x -> file.WriteLine(x))


    member _this.GoDepth(depth) =
        printf "%A\n" depth

    member _this.Dispose =
        file.Close()  
  




