module Trainer
open Microsoft.ML

let sdcaBinaryClassification featureCol labelCol (mlContext : MLContext) =
    mlContext.
      BinaryClassification.
      Trainers.
      StochasticDualCoordinateAscent (
        features = featureCol,
        label = labelCol
    )