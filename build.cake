#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=xunit.runner.console&version=2.2.0"

#addin "Cake.FileHelpers"

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release");
var artifactsDir    = Directory("./artifacts");
var solution        = "./src/Eru.sln";
GitVersion versionInfo = null;



Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
});

Task("SetVersionInfo")
    .IsDependentOn("Clean")
    .Does(() =>
{
    versionInfo = GitVersion(new GitVersionSettings {
        RepositoryPath = "."
    });
    Information(versionInfo.NuGetVersionV2);
});

Task("RestorePackages")
    .IsDependentOn("SetVersionInfo")
    .Does(() =>
{
    NuGetRestore(solution);
});

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    MSBuild(solution, new MSBuildSettings 
    {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = configuration,
        ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.NuGetVersionV2)
    });
});


Task("RunTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testAssemblies = GetFiles("./src/**/bin/Release/*.Tests.dll");
    XUnit2(testAssemblies,
        new XUnit2Settings {
            Parallelism = ParallelismOption.All,
            HtmlReport = true,
            NoAppDomain = true,
            OutputDirectory = "./artifacts"
        });
});

Task("CopyPackages")
    .IsDependentOn("Build")
    .Does(() =>
{
    var files = GetFiles("./src/Eru*/**/*.nupkg");
    CopyFiles(files, "./artifacts");

});

Task("NuGetPublish")
    .IsDependentOn("CopyPackages")
    .Does(() =>
    {        
        var APIKey = EnvironmentVariable("NUGETAPIKEY");
        var packages = GetFiles("./artifacts/*.nupkg");

        NuGetPush(packages, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = APIKey
        });
    });

Task("Default")
    .IsDependentOn("RunTests")
    .IsDependentOn("NuGetPublish");

RunTarget(target);