open System
open System.IO
#r @"../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
open FSharp.Data

let bingImageSearchKey = "<-*insert-bing-image-search-api-key*->"
let bingImageSearchUri = "https://api.cognitive.microsoft.com/bing/v7.0/images/search"

let imagesPerPage = 35
let startPage = 1

let searchTermsWithNoOfPages = 
  [|
    2, "beard"
    2, "elf"
    2, "hat"
    2, "nature"
    2, "old man"
    2, "person"
    22, "santa claus"
    2, "shopping"
    2, "skier"
    2, "sport"
    2, "wheres wally"
    2, "winter"
  |]

type BingImageSearchResult = JsonProvider<"../Data/json-samples/bing-image-search-response.json">

let bingImageSearch searchTerm count offset =
  async {
    let! response =
      Http.AsyncRequestString
        (
           bingImageSearchUri,
           httpMethod = "GET",
           query = [ 
             "q", Uri.EscapeDataString (searchTerm)
             "count", Uri.EscapeDataString (sprintf "%i" count)
             "offset", Uri.EscapeDataString (sprintf "%i" offset)
           ],
           headers = [ "Ocp-Apim-Subscription-Key", bingImageSearchKey ]
        )
    return response
  }

let createFileName path pageNo (searchTerm  : string) =
  let filename = sprintf "%s.page%i.json" (searchTerm.Replace (' ', '-')) pageNo
  Path.Combine (path, filename)

let rec searchAndWriteFile maxNoOfPages count offset searchTerm =
  async {
    let! jsonStringResult =
      bingImageSearch
        searchTerm
        count
        offset

    let pageNo = (offset / count)
    
    let filename = searchTerm |> createFileName @"../data/input/image-search-results/" pageNo

    use sw = new StreamWriter (filename)

    do! sw.WriteLineAsync jsonStringResult |> Async.AwaitTask

    sw.Close ()

    let searchResult = BingImageSearchResult.Parse jsonStringResult

    let nextOffset = searchResult.NextOffset

    if (nextOffset <= maxNoOfPages * count) then
      return! searchAndWriteFile maxNoOfPages count nextOffset searchTerm
    else
      return ()    
  }

for (noOfPages, searchTerm) in searchTermsWithNoOfPages do
  searchTerm
  |> searchAndWriteFile noOfPages imagesPerPage (startPage * imagesPerPage)
  |> Async.RunSynchronously

  Async.Sleep (1000) // prevent a "too many requests" response from API
  |> Async.RunSynchronously
