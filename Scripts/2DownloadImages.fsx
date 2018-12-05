open System
open System.Drawing
open System.IO
open System.Threading.Tasks
open System.Net
#r @"../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
open FSharp.Data

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

let downloadAndSaveImage category filename imageCount url encoding =
  async {
    try
      let! response = Http.AsyncRequestStream url

      let imagePath =
        sprintf @"../Data/input/images/%s/%s.image%i.%s" category filename imageCount encoding

      use fs = new FileStream (imagePath, FileMode.Create, FileAccess.Write)

      do! response.ResponseStream.CopyToAsync fs |> Async.AwaitTask   
    with
      | :? WebException as ex ->
        printfn "Category: %s. Image Count: %i. Failed to retrieve image." category imageCount 
  }

let getImagesForImageSearchResult filePath category =
  async {
    let! searchResult = BingImageSearchResult.AsyncLoad filePath

    let resultFilename = Path.GetFileName filePath

    let filename = resultFilename.Split([|".json"|], StringSplitOptions.RemoveEmptyEntries) |> Seq.head

    do! searchResult.Value
        |> Seq.map (fun value -> value.ContentUrl, value.EncodingFormat)
        |> Seq.mapi (fun i (url, encoding) -> (downloadAndSaveImage category filename i url encoding))
        |> Async.Parallel
        |> Async.Ignore
  }

let getFilename category pageNo =
  sprintf "%s.page%i.json" category pageNo

let getImagesForPagedSearchResults pageRange category =
  for pageNo in pageRange do
    let filename = pageNo |> getFilename category

    getImagesForImageSearchResult
            (sprintf @"../Data/input/image-search-results/%s" filename)
            category
    |> Async.RunSynchronously   

searchTermsWithNoOfPages
|> Seq.iter (
    fun (noOfPages, category) ->
      getImagesForPagedSearchResults [1..noOfPages] category)