module Parser

#if INTERACTIVE
#r @"..\packages\FSharp.Data\lib\net45\FSharp.Data.dll"
#endif
open FSharp.Data
open System.Text
open System.Collections.Generic
open System.Numerics

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


    let getWordsFromsUrl(url) =
        let results = HtmlDocument.Load(string url, encoding)
        let body = results.Descendants ["body"]
        if Seq.length(body) > 0 then 
            words.Clear()
            let node = body |> Seq.head
            readNode(node)
        words

    let getUrlsPairs(urls:List<string>) =
        let pairList = new List<string * string>()
        for i = 0 to urls.Count-1 do
            let url = urls.[i]
            for k = i+1 to urls.Count-1 do
                let pair = (url,urls.[k])
                pairList.Add(pair)
        pairList    

    member _this.SetUrlsToVisit =
        let tempUrlsToVisit = new List<string>()
        let tempUrls = new List<string>()
        let urlsToParse = new List<string>()
        if depthLevel <= 1 then
            urlsToVisit.Add(parseUrl)
        else    
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
            words.Clear()
            let results = HtmlDocument.Load(string url, encoding)
            let body = results.Descendants ["body"]
            if Seq.length(body) > 0 then 
                let node = body |> Seq.head
                readNode(node)

                if writeToConsole then 
                    printf "\n%A:\n" url
                    words
                    |> Seq.countBy (fun s -> s.Trim())
                    |> Seq.iter (fun (x) -> printf "%A\n" x)

                if writeToFile then
                    file.WriteLine(url)
                    words
                    |> Seq.countBy (fun s -> s.Trim())
                    |> Seq.sortBy (snd >> (~-))
                    |> Seq.iter (fun x -> file.WriteLine(x))


    member _this.CountCosinus =
        let pairList = getUrlsPairs(urlsToVisit)
        for (x,y) in pairList do
            let allWords = new List<string>()
            let tmpWords1 = new List<string>()
            let tmpWords2 = new List<string>()
            tmpWords1.AddRange(getWordsFromsUrl(x))
            tmpWords2.AddRange(getWordsFromsUrl(y))
            let words1 = tmpWords1 |> Seq.countBy (fun s -> s.Trim())
            let words2 = tmpWords2 |> Seq.countBy (fun s -> s.Trim())

            words1 |> Seq.iter(fun (x,y) -> allWords.Add(x)) 
            words2 |> Seq.iter(fun (x,y) -> allWords.Add(x))
            let uniqueWords = Seq.distinct(allWords)
            let vector1 = seq { for word in uniqueWords -> words1 |> Seq.tryFindIndex( fun (x,y) -> x.Equals(word)) |> fun(x) -> if x=Option.None then -1 else Option.get(x) |> (fun(x) -> if x=(-1) then ("",0) else (Seq.item x words1)) |> fun(x,y) -> y }
            let vector2 = seq { for word in uniqueWords -> words2 |> Seq.tryFindIndex( fun (x,y) -> x.Equals(word)) |> fun(x) -> if x=Option.None then -1 else Option.get(x) |> (fun(x) -> if x=(-1) then ("",0) else (Seq.item x words2)) |> fun(x,y) -> y }
            // printf "%A\n" x
            // printf "%A\n" y
            // vector1 |> Seq.iter (fun (x) -> printf "%A, " x)
            // printf "\n"
            // vector2 |> Seq.iter (fun (x) -> printf "%A, " x)
                


    member _this.Dispose =
        file.Close()  
  

