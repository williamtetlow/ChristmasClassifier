namespace Domain.Configuration

open Common

[<Struct>]
type ImageSettings =
  val mutable ImageHeight : int
  val mutable ImageWidth : int
  val mutable Mean : float32
  val mutable Scale : float32
  val mutable ChannelsLast : bool
  new (channelsLast) =
      { ImageHeight = 224
        ImageWidth = 224
        Mean = float32 117
        Scale = float32 1
        ChannelsLast = channelsLast }

type ModelConfig =
    { TrainDataLocation : string
      TestDataLocation : string
      ImagesFolder : string
      InputModelLocation : string
      OutputModelLocation : string
      ImageSettings : ImageSettings }
    member x.PrintDetails () =
      Console.header [| "Read model" |]
      printfn "Model location: %s" x.InputModelLocation
      printfn "Images folder: %s" x.ImagesFolder
      printfn "Training file: %s" x.TrainDataLocation
      printfn "Testing file: %s" x.TestDataLocation
      printfn "Default parameters: image size=(%i, %i), image mean: %f"
        x.ImageSettings.ImageWidth
        x.ImageSettings.ImageHeight
        x.ImageSettings.Mean

    static member Create (trainDataLocation, testDataLocation, imagesFolder, inputModelLocation, outputModelLocation, imageSettings) =
      {
        TrainDataLocation = trainDataLocation
        TestDataLocation = testDataLocation
        ImagesFolder = imagesFolder
        InputModelLocation = inputModelLocation
        OutputModelLocation = outputModelLocation
        ImageSettings = imageSettings
      }      

  

