module Domain.ImageData

open Microsoft.ML
open Microsoft.ML.Runtime.Api
open System.ComponentModel.Composition

[<CLIMutable>]
type ImageInput =
    { [<Column("0")>]
      ImagePath : string
      [<Column("1")>]
      Label : string }

[<CLIMutable>]
type ImagePrediction =
    { ImagePath : string
      [<ColumnName("PredictedLabel")>]
      IsSanta : bool
      Score : float32
      Probability : float32
      softmax2_pre_activation : float32[] }

module CustomMapping =
  type InputRow () =
    [<DefaultValue>]
    val mutable Label : string

  type OutputRow () =
    [<DefaultValue>]
    val mutable Label : bool

  [<CLIMutable>]
  type InputMapping =
    {
      [<Import>]
      MLContext : MLContext
    }
    with
    static member IsSantaMapping = 
      System.Action<InputRow, OutputRow>(
        fun input ->
          fun output ->
            output.Label <- input.Label = "santaclaus")     

    [<Export("IsSantaMapping")>]
    member this.CustomIsSantaTransform =
      this.MLContext.Transforms.
        CustomMappingTransformer<InputRow, OutputRow>
          (
            mapAction = InputMapping.IsSantaMapping,
            contractName = "IsSantaMapping"
          )
