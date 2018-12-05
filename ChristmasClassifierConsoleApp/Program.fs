open Common
open Domain.Configuration
open Domain.ImageData.CustomMapping
open Microsoft.ML
open Microsoft.ML.Runtime.ImageAnalytics
open Microsoft.ML.Runtime.Data
open System
open System.IO

let dataLoader mlContext =
    TextLoader (mlContext,
      TextLoader.Arguments (
        Column = [|
          TextLoader.Column ("ImagePath", Nullable DataKind.Text, 0)
          TextLoader.Column ("Label", Nullable DataKind.Text, 1)
        |] 
      ))

let read (dataPath : string) (dataLoader : TextLoader) =
    dataLoader.Read dataPath

let buildTrainAndSaveModel (config : ModelConfig) (mlContext : MLContext) =
    config.PrintDetails ()

    let trainingDataView =
        mlContext
        |> dataLoader
        |> read config.TrainDataLocation


    let testDataView =
        mlContext
        |> dataLoader
        |> read config.TestDataLocation    

    Console.header [| "Creating data pipeline" |]

    let dataPipeline =
        mlContext.Transforms.CustomMapping<InputRow, OutputRow> (
          InputMapping.IsSantaMapping,
          "IsSantaMapping"
        )

    Console.header [| "Building classification model" |]

    let modelBuilder =
        dataPipeline
        |> ModelBuilder.create mlContext
        |> ModelBuilder.add
            (Estimator.ImageAnalytics.imageLoading
              config.ImagesFolder
              [| struct ("ImagePath", "ImageReal") |])

        |> ModelBuilder.add
            (Estimator.ImageAnalytics.imageResizing
              "ImageReal"
              "ImageReal"
              config.ImageSettings.ImageHeight
              config.ImageSettings.ImageWidth)

        |> ModelBuilder.add
            (Estimator.ImageAnalytics.imagePixelExtracting
              [|
                ImagePixelExtractorTransform.
                  ColumnInfo (
                    "ImageReal",
                    "input",
                    ImagePixelExtractorTransform.ColorBits.Rgb,
                    config.ImageSettings.ChannelsLast,
                    config.ImageSettings.Scale,
                    config.ImageSettings.Mean)
              |])

        |> ModelBuilder.add
            (Estimator.Tensorflow.tensorFlow
              config.InputModelLocation
              [| "input" |]
              [| "softmax2_pre_activation" |])

        |> ModelBuilder.add
            (Trainer.sdcaBinaryClassification
              "softmax2_pre_activation"
              "Label")

    Console.header [| "Training classification model" |]
    
    let trainedModel =
        modelBuilder
        |> ModelBuilder.train trainingDataView

    Console.header [| "Evaluating trained model's accuracy with test data" |]

    let metrics =
        (trainedModel, modelBuilder)
        |> ModelBuilder.evaluateBinaryClassificationModel testDataView "Label" "Score"

    Console.printBinaryClassificationMetrics
      "Stochastic Dual Coordinate Ascent"
      metrics

    Console.header [| "Saving model to local file" |]

    (trainedModel, modelBuilder)
    |> ModelBuilder.saveModelAsFile config.OutputModelLocation
        
[<EntryPoint>]
let main argv =
    let config =
      ModelConfig.Create (
        trainDataLocation = Path.GetFullPath @"../Data/input/images/train-images.tsv",
        testDataLocation = Path.GetFullPath @"../Data/input/images/test-images.tsv",
        imagesFolder = Path.GetFullPath @"../Data/input/images",
        inputModelLocation = Path.GetFullPath @"../Data/input/model/inception5h/tensorflow_inception_graph.pb",
        outputModelLocation = Path.GetFullPath @"../Data/output/ChristmasClassifier.zip",
        imageSettings = ImageSettings (true))

    MLContext(seed = Nullable 1)
    |> buildTrainAndSaveModel config

    Console.header [| "End of training process" |]

    Console.pressAnyKey ()

    0 // return an integer exit code
