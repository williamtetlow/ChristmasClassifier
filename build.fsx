#r "paket:
nuget FSharp.Core
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli //"

#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "Facades/netstandard"
#r "netstandard"
#endif

open Fake.Core
open Fake.IO
open Fake.DotNet

let projectPath = "ChristmasClassifierConsoleApp" |> Path.getFullName
let projectFile = "ChristmasClassifierConsoleApp.fsproj"

let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson ()

Target.create "InstallDotNetCore" (fun _ -> 
  let setInstallParams (options : DotNet.CliInstallOptions) =
    { options with
        Version = DotNet.Version dotnetcliVersion 
    }

  DotNet.install setInstallParams |> ignore
)

Target.create "Clean" (fun _ ->
  Trace.log " --- Cleaning --- "

  DotNet.exec id "clean" ""
  |> ignore
)

Target.create "Build" (fun _ ->
  Trace.log " --- Building --- "

  DotNet.build id ""
)

Target.create "Run" (fun _ ->
  Trace.log " --- Run ---"

  let setRunParams (defaultRunParams : DotNet.Options) =
    { defaultRunParams with
        WorkingDirectory = projectPath
    }

  DotNet.exec setRunParams "run" projectFile
  |> ignore 
)

Target.create "Publish" (fun _ ->
  Trace.log " --- Publishing app --- "

  let setPublishParams (defaultPublishParams :DotNet.PublishOptions) = 
    { defaultPublishParams with
        Common = { defaultPublishParams.Common with WorkingDirectory = projectPath }
    }

  DotNet.publish setPublishParams projectFile
)

Target.createFinal "Init" (fun _ -> 
  Trace.log " --- Initialisation is done --- "
)

Target.createFinal "BuildAndPublish" (fun _ ->
  Trace.log " --- Fake script is done --- "
)

open Fake.Core.TargetOperators

"Clean"
  ==> "InstallDotNetCore"
  ==> "Init"

"Clean"
  ==> "Build"
  ==> "Publish"
  ==> "BuildAndPublish"

"Clean"
  ==> "Build"
  ==> "Run"

Target.runOrDefault "Build"