open System
open System.IO
open System.Text.RegularExpressions
#r @"../packages/FSharp.Data/lib/net45/FSharp.Data.dll"
open FSharp.Data

let imageBasePath = Path.GetFullPath @"../Data/input/images/"

let trainImagesTsvPath = Path.GetFullPath @"../Data/input/images/train-images.tsv"

let testImagesTsvPath = Path.GetFullPath @"../Data/input/images/test-images.tsv"

let isImageFile filename =
  Regex.IsMatch (filename, @".jpg|.jpeg|.png$")

let toRelativeFilename basePath filename =
  let fileUri = Uri filename
  let baseUri = Uri basePath

  (baseUri.MakeRelativeUri fileUri).ToString ()

let rec getAllImageFileNames dirs =
  if dirs |> Seq.isEmpty then Seq.empty else
    seq { 
          yield! dirs
            |> Seq.collect Directory.EnumerateFiles
            |> Seq.filter isImageFile
            |> Seq.map (toRelativeFilename imageBasePath)
          yield! dirs
            |> Seq.collect Directory.EnumerateDirectories
            |> getAllImageFileNames 
        }

type ImageInfo = {
  path: string
  label: string
}

let filenameToLabel =
  function
  | (santaClauseImg : string) 
      when santaClauseImg.Contains ("santa-claus") -> "santaclaus"
  | _ -> "notsantaclaus"

let filenameToImageInfo filename =
  if String.IsNullOrEmpty (filename) then
    None
  else
    Some {
      path = filename
      label = filename |> filenameToLabel
    }

type ImageTsv =
  CsvProvider<
    Schema = "string, string",
    Separators = "\t",
    HasHeaders = false
  >

let imageInfoToRow imageInfo =
  ImageTsv.Row (imageInfo.path, imageInfo.label)

let saveTsvString path tsvString =
  File.WriteAllText (path, tsvString)

let saveTsvForFileNames tsvFilename filenames =
  let rows =
    filenames
    |> Seq.map filenameToImageInfo
    |> Seq.map (Option.map imageInfoToRow)
    |> Seq.choose id
    |> Seq.toList

  use imageTsv = new ImageTsv (rows)

  imageTsv.SaveToString ()
  |> saveTsvString tsvFilename

let createTrainAndTestTsvFromFilenames filenames =
  let groupedByCategory =
    filenames
    |> List.groupBy (fun (filename : string) -> filename.Split ('/') |> Seq.head)

  let testFiles, trainFiles =
    groupedByCategory 
    |> List.map (fun (_, filesForGroup) -> filesForGroup |> List.splitAt (List.length filesForGroup / 5))
    |> List.reduce
        (fun (test, train) (testForGroup, trainForGroup) ->
          (List.append test testForGroup), (List.append train trainForGroup))
    
  testFiles
  |> saveTsvForFileNames testImagesTsvPath

  trainFiles
  |> saveTsvForFileNames trainImagesTsvPath

[| imageBasePath |]
|> getAllImageFileNames
|> Seq.toList
|> createTrainAndTestTsvFromFilenames
