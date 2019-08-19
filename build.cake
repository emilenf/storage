var target = Argument<string>("Target", "Default");
var configuration = Argument<string>("Configuration", "Release");
bool publishWithoutBuild = Argument<bool>("PublishWithoutBuild", false);

var artifactsDirectory = Directory("./artifacts");
var testResultDir = "./temp/";
var isRunningOnBuildServer = !BuildSystem.IsLocalBuild;

var msBuildSettings = new DotNetCoreMSBuildSettings();


if (HasArgument("BuildNumber"))
{
    msBuildSettings.WithProperty("BuildNumber", Argument<string>("BuildNumber"));
    msBuildSettings.WithProperty("VersionSuffix", "alpha" + Argument<string>("BuildNumber"));
}

if (HasArgument("VersionPrefix"))
{
    msBuildSettings.WithProperty("VersionPrefix", Argument<string>("VersionPrefix"));
}

if (HasArgument("PythonVersion"))
{
    msBuildSettings.WithProperty("PythonVersion", Argument<string>("PythonVersion"));
}

Task("Clean-Artifacts")
    .Does(() =>
{
    CleanDirectory(artifactsDirectory);
});

Task("Build")
    .Does(() =>
{
    var dotNetCoreSettings = new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings
            };
    DotNetCoreBuild("Cmdty.Storage.sln", dotNetCoreSettings);
});

Task("Test-C#")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Cleaning test output directory");
    CleanDirectory(testResultDir);

    var projects = GetFiles("./tests/**/*.Test.csproj");
    
    foreach(var project in projects)
    {
        Information("Testing project " + project);
        DotNetCoreTest(
            project.ToString(),
            new DotNetCoreTestSettings()
            {
//                ArgumentCustomization = args=>args.Append($"/p:CollectCoverage=true /p:CoverletOutputFormat=cobertura"),
                Logger = "trx",
                ResultsDirectory = testResultDir,
                Configuration = configuration,
                NoBuild = true
            });
    }
});

Task("Default")
	.IsDependentOn("Test-C#");

Task("CI")
	.IsDependentOn("Test-C#");

RunTarget(target);