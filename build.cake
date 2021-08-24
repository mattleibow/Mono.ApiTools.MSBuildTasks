var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var prerelease = Argument("prerelease", true);

// a bit of logic to create the version number:
//  - input                     = 1.2.3.4
//  - package version           = 1.2.3
//  - preview package version   = 1.2.3-preview.4
var version         = Version.Parse(Argument("packageversion", "1.0.0.0"));
var previewNumber   = version.Revision;
var assemblyVersion = $"{version.Major}.0.0.0";
var fileVersion     = $"{version.Major}.{version.Minor}.{version.Build}.0";
var infoVersion     = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
var packageVersion  = $"{version.Major}.{version.Minor}.{version.Build}";
var previewVersion  = packageVersion + "-preview." + previewNumber;

Task("Build")
    .Does(() =>
{
    var settings = new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
        .WithRestore()
        .WithProperty("Version", assemblyVersion)
        .WithProperty("FileVersion", fileVersion)
        .WithProperty("InformationalVersion", infoVersion);

    MSBuild("Mono.ApiTools.MSBuildTasks.sln", settings);
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new MSBuildSettings()
        .SetConfiguration(configuration)
        .SetVerbosity(Verbosity.Minimal)
        .WithProperty("Version", assemblyVersion)
        .WithProperty("FileVersion", fileVersion)
        .WithProperty("InformationalVersion", infoVersion)
        .WithProperty("PackageOutputPath", MakeAbsolute((DirectoryPath)"./output/").FullPath)
        .WithProperty("PackageVersion", prerelease ? previewVersion : packageVersion)
        .WithTarget("Pack");


    MSBuild("Mono.ApiTools.MSBuildTasks/Mono.ApiTools.MSBuildTasks.csproj", settings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Running unit tests...");
    DotNetCoreTest("Mono.ApiTools.MSBuildTasks.Tests/Mono.ApiTools.MSBuildTasks.Tests.csproj", new DotNetCoreTestSettings {
        Logger = "trx"
    });
});

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Pack")
    .IsDependentOn("Test");

RunTarget(target);
