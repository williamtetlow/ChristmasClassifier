module Common 

open System

module Printf =
  let private consoleColor (color : ConsoleColor) =
      let current = Console.ForegroundColor
      Console.ForegroundColor <- color
      { new IDisposable with
              member x.Dispose() = Console.ForegroundColor <- current }

  let cprintf color str =
      Printf.kprintf (fun s -> use c = consoleColor color in printf "%s" s) str
  
  let cprintfn color str =
      Printf.kprintf (fun s -> use c = consoleColor color in printfn "%s" s) str

[<RequireQualifiedAccess>]
module Console =
  open Printf
  open Microsoft.ML.Runtime.Data

  let private printLines lines =
      lines
      |> Seq.iter ((printfn "%s"))

  let private printColoredLines color lines =
      lines
      |> Seq.iter (cprintfn color "%s")  

  let private withBorder lines =
      let maxLineLength =
        lines
        |> Seq.map String.length
        |> Seq.max

      let border = 
        [| String.replicate maxLineLength "#" |]

      border
      |> Seq.append lines
      |> Seq.append border
      
  let header lines =
      let headerColor = ConsoleColor.Yellow

      lines
      |> withBorder
      |> printColoredLines headerColor

  let pressAnyKey () =
      cprintfn
        ConsoleColor.Green
        "%sPress any key to finish."
        Environment.NewLine

      Console.ReadKey true |> ignore

  let error lines =
      let exceptionColor = ConsoleColor.Red

      [| "EXCEPTION" |]
      |> withBorder
      |> Seq.iter (cprintfn exceptionColor "%s")

      lines
      |> Seq.iter (printfn "%s")

  let printBinaryClassificationMetrics name (metrics : BinaryClassifierEvaluator.Result) =
      let metricColor = ConsoleColor.Magenta

      [|
        sprintf "Accuracy: %.2f%%" (metrics.Accuracy * 100.)
        sprintf "Auc:      %.2f%%" (metrics.Auc * 100.)
        sprintf "F1Score:  %.2f%%" (metrics.F1Score * 100.)
      |]
      |> withBorder
      |> printColoredLines metricColor
