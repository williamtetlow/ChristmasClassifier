[<RequireQualifiedAccess>]
module ModelBuilder
open Microsoft.ML
open Microsoft.ML.Core.Data
open Microsoft.ML.Runtime.Data
open System.IO

let private downcastPipeline (pipeline : IEstimator<'a>) =
    match pipeline with
    | :? IEstimator<ITransformer> as p -> p
    | _ -> failwith "The pipeline has to be an instance of IEstimator<ITransformer>."

let create (mlContext : MLContext) (pipeline : IEstimator<'a>) =
    (mlContext, pipeline |> downcastPipeline)

let private append (estimator : IEstimator<'a>) (pipeline : IEstimator<'b>)  =
    let pipeline' = pipeline |> downcastPipeline
    pipeline'.Append estimator

let add estimatorFn (mlContext : MLContext, pipeline : IEstimator<'a>) =
    let estimator =
      mlContext
      |> estimatorFn

    let newPipeline =
      pipeline
      |> append estimator

    (mlContext, newPipeline)

let train (trainingData : IDataView) (mlContext : MLContext, pipeline : IEstimator<'a>) =
    pipeline.Fit trainingData :> ITransformer

let crossValidate (trainingData : IDataView) (mlContext : MLContext, pipeline : EstimatorChain<'a>) =
    let estimator = pipeline |> downcastPipeline
    let results = mlContext.BinaryClassification.CrossValidate(trainingData, estimator, numFolds = 5)
    let auc =
      results
      |> Seq.map (fun struct(metrics, _, _) -> metrics.Auc)
      |> Seq.average

    printfn "The AUC is %f (Should be between 0.5 and 1.0, the higher the better!)" auc

let private checkTrained (trainedModel : ITransformer) =
    if (trainedModel = null) then
        failwith "Cannot test before training. Call Train() first."

let evaluateBinaryClassificationModel (testData : IDataView) label score (trainedModel : ITransformer, (mlContext : MLContext, _)) =
    checkTrained trainedModel
    let predictions = trainedModel.Transform testData
    mlContext.BinaryClassification.Evaluate(predictions, label, score)

let saveModelAsFile persistedModelPath (trainedModel : ITransformer, (mlContext : MLContext, _)) =
    checkTrained trainedModel

    use fs = new FileStream(persistedModelPath, FileMode.Create, FileAccess.Write, FileShare.Write)
    mlContext.Model.Save(trainedModel, fs);
    printfn "The model is saved to %s" persistedModelPath



   







  