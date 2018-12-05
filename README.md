# ChristmasClassifier
Binary Image Classifier in F# using ML.NET

# Requirements
- [Mono](https://www.mono-project.com/) on Mac or Linux `brew install mono`
- .NET Core 2.1
- [FAKE dotnet cli tool](https://fake.build/fake-gettingstarted.html)
- On Mac or Linux `brew install mono-libgdiplus`

# Initialising Project

After installing the [FAKE dotnet cli tool](https://fake.build/fake-gettingstarted.html) run

````shell
fake run build.fsx
fake build target Init
````

# Downloading Training Data

Sign up to [Bing Image Search API](https://azure.microsoft.com/en-gb/services/cognitive-services/bing-image-search-api/) and add API key to [1DownloadImageSearchResults](./Scripts/1DownloadImageSearchResults.fsx)

Run the three fsx scripts inside the Scripts directory in order.

- [1DownloadImageSearchResults.fsx](./Scripts/1DownloadImageSearchResults.fsx)
- [2DownloadImages.fsx](./Scripts/2DownloadImages.fsx)
- [3CreateImageTsv.fsx](./Scripts/3CreateImageTsv.fsx)


# Train and Evaulate the Model

````fsharp
fake build target Run
````
