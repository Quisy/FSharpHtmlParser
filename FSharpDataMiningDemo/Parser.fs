module Parser

#if INTERACTIVE
#r @"..\packages\FSharp.Data\lib\net45\FSharp.Data.dll"
#endif
open FSharp.Data
open System.Text
open System.Collections.Generic

type Parser(url, console, writeToFile, filePath, depthLevel) = 
    let parseUrl = string url
    let writeToConsole = console;
    let encoding = Encoding.GetEncoding("Windows-1250")
    //let results = HtmlDocument.Load(parseUrl, encoding)

    let file = System.IO.File.CreateText(filePath) 


    let visitedUrls = new List<string>()

    let urlsToVisit = new List<string>()
    
    let getLinksFromUrl(url) = 
        let results = HtmlDocument.Load(string url, encoding)
        results.Descendants ["a"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("href")
               |> Option.map (fun a -> a.Value())
        )

    let getImagesFromUrl(url) = 
        let results = HtmlDocument.Load(string url, encoding)
        results.Descendants ["img"]
        |> Seq.choose (fun x -> 
               x.TryGetAttribute("src")
               |> Option.map (fun a -> a.Value())
        )

    let getScriptsFromUrl(url) = 
        let results = HtmlDocument.Load(string url, encoding)
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

    let filterUrl(url) =
        //printf "%A\n" url
        if not (url.ToString().Contains(parseUrl)) then 
            if (url.ToString().Contains("http")) then ""
            elif (url.ToString().Contains("mailto:")) then ""
            else System.String.Concat([|parseUrl; "/"; url|])        
        elif (url.ToString().Contains(parseUrl)) then
            url
        else ""
            
    let getUrlsFromUrl(url) =
        let urll = filterUrl(url)
        if (urll <> "") then
            let docResult = HtmlDocument.Load(string urll, encoding) 
            let urls = docResult.Descendants ["a"] |> Seq.choose (fun (x:HtmlNode) -> x.TryGetAttribute("href")) |> Seq.map (fun (a:HtmlAttribute) -> a.Value())
            Seq.distinct(urls)
        else
            Seq.empty<string>    
        

    member _this.SetUrlsToVisit =
        let tempUrlsToVisit = new List<string>()
        let tempUrls = new List<string>()
        let urlsToParse = new List<string>()
        urlsToParse.Add(parseUrl)
        for i = 1 to depthLevel do
            for link in urlsToParse do
                let tempLinks = getUrlsFromUrl(link)
                tempUrls.AddRange(tempLinks)
            tempUrlsToVisit.AddRange(tempUrls)
            urlsToParse.Clear()
            urlsToParse.AddRange(tempUrls)
            tempUrls.Clear()
        urlsToParse.Clear()
        let distinctUrls = Seq.distinct(tempUrlsToVisit)
        //urlsToVisit.AddRange(distinctUrls)
        for url in distinctUrls do
            let urll = filterUrl(url)
            if (urll <> "") then urlsToVisit.Add(urll)
    member _this.GetImages =
        if writeToConsole then printf "\nIMAGES:\n"
        if writeToFile then file.WriteLine("IMAGES:")
        for url in urlsToVisit do
            if writeToConsole then printf "\n%A:\n" url
            let images = getImagesFromUrl(url)
            if writeToConsole then images |> Seq.iter (fun x -> printf "-%A\n" x)
            if writeToFile then 
                file.WriteLine(url)
                images |> Seq.iter (fun x -> file.WriteLine(x))
        
    member _this.GetLinks =
        if writeToConsole then printf "\nLINKS:\n"
        if writeToFile then file.WriteLine("nLINKS:")
        for url in urlsToVisit do
            if writeToConsole then printf "\n%A:\n" url
            let links = getLinksFromUrl(url)
            if writeToConsole then links |> Seq.iter (fun x -> printf "-%A\n" x)
            if writeToFile then 
                file.WriteLine(url)
                links |> Seq.iter (fun x -> file.WriteLine(x))

    member _this.GetScripts =
        if writeToConsole then printf "\nSCRIPTS:\n"
        if writeToFile then file.WriteLine("nSCRIPTS:")
        for url in urlsToVisit do
            if writeToConsole then printf "\n%A:\n" url
            let scripts = getScriptsFromUrl(url)
            if writeToConsole then scripts |> Seq.iter (fun x -> printf "-%A\n" x)
            if writeToFile then 
                file.WriteLine(url)
                scripts |> Seq.iter (fun x -> file.WriteLine(x))

    member _this.PrintUrlsToVisit =
        printf "\n\nURLS TO VISIT: \n"
        urlsToVisit |> Seq.iter (fun x -> printf "%A\n" x)

    member _this.CountWords =
        if writeToConsole then printf "\nWORDS:\n"
        if writeToFile then file.WriteLine("nWORDS:")
        for url in urlsToVisit do
            let results = HtmlDocument.Load(string url, encoding)
            let body = results.Descendants ["body"]
            let node = body |> Seq.head
            readNode(node)

            if writeToConsole then 
                printf "\n%A:\n" url
                words
                |> Seq.countBy (fun s -> s.Trim())
                |> Seq.sortBy (snd >> (~-))
                |> Seq.iter (fun x -> printf "%A\n" x)

            if writeToFile then
                file.WriteLine(url)
                words
                |> Seq.countBy (fun s -> s.Trim())
                |> Seq.sortBy (snd >> (~-))
                |> Seq.iter (fun x -> file.WriteLine(x))


    member _this.GoDepth(depth) =
        printf "%A\n" depth

    member _this.Dispose =
        file.Close()  
  




