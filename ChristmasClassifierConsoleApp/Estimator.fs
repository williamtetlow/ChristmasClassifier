module Estimator

module ImageAnalytics =
  open Microsoft.ML.Runtime.ImageAnalytics

  let imageLoading imagesFolder columns mlContext =
      ImageLoadingEstimator (
        mlContext,
        imagesFolder,
        columns)

  let imageResizing inputCol outputCol imgHeight imgWidth mlContext =
      ImageResizingEstimator (
        mlContext,
        inputCol,
        outputCol,
        imgHeight,
        imgWidth)

  let imagePixelExtracting columns mlContext =
      ImagePixelExtractingEstimator (
        mlContext,
        columns)

module Tensorflow =
  open Microsoft.ML.Transforms

  let tensorFlow (modelLocation : string) inputs outputs mlContext =
      TensorFlowEstimator (
        mlContext,
        modelLocation,
        inputs,
        outputs)