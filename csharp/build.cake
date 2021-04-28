#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var csDir = ".";
var gradleRootDir = "..";
var slnPath = $"{csDir}/HdDateTime.sln";
var mainProjectName = "EPAM.Deltix.HdTime";
var testProjectName = $"{mainProjectName}.Tests";
var buildDir = Directory($"{csDir}/{mainProjectName}/bin") + Directory(configuration);

// Parse version from gradle.properties
var gradleProperties = new Dictionary<String, String>();
foreach (var row in System.IO.File.ReadAllLines($"{gradleRootDir}/gradle.properties"))
    gradleProperties.Add(row.Split('=')[0], String.Join("=",row.Split('=').Skip(1).ToArray()));

var version = gradleProperties["version"];
var index = version.IndexOf("-");
var dotNetVersion = (index > 0 ? version.Substring(0, index) : version) + ".0";

//////////////////////////////////////////////////////////////////////
// Helpers
//////////////////////////////////////////////////////////////////////

String prjDir(String name) { return $"{csDir}/{name}"; }
String prjPath(String name) { return $"{prjDir(name)}/{name}.csproj"; }
String binDir(String name) { return $"{prjDir(name)}/bin/{configuration}"; }
String objDir(String name) { return $"{prjDir(name)}/obj/{configuration}"; }

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean(slnPath,
        new DotNetCoreCleanSettings { Configuration = configuration }
    );

    CleanDirectory(binDir(mainProjectName));
});


Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(slnPath);
});


Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var settings =  new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoRestore = true,
        NoDependencies = true,
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", version)
            .WithProperty("FileVersion", dotNetVersion)
            .WithProperty("AssemblyVersion", dotNetVersion)
    };

    if (!IsRunningOnWindows())
        settings.Framework = "netstandard2.0";
    DotNetCoreBuild(prjPath(mainProjectName), settings);

    if (!IsRunningOnWindows())
        settings.Framework = "netcoreapp2.0";
    DotNetCoreBuild(prjPath(testProjectName), settings);
});


Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings()
    {
        Configuration = configuration,
        NoRestore = true,
        NoBuild = true
    };

    if (!IsRunningOnWindows())
        settings.Framework = "netcoreapp2.0";

    Information("Running tests with .NET Core");
    DotNetCoreTest(prjPath(testProjectName), settings);

    // Prevent NUnit tests from running on platforms without .NET 4.0
    var glob = $"{binDir(testProjectName)}/net40/{testProjectName}.exe";
    Information(glob);
    if (IsRunningOnWindows() && GetFiles(glob).Count > 0)
    {
        Information("Running NUnit tests with NUnit & .NET Framework 4.0");
        NUnit3(glob);
    }
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = $"{csDir}/artifacts/",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("Version", version)
            .WithProperty("FileVersion", dotNetVersion)
            .WithProperty("AssemblyVersion", dotNetVersion)
    };

    DotNetCorePack($"{csDir}", settings);
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{
    var url = "https://packages.deltixhub.com/nuget/" + (EnvironmentVariable("FEED_BASE_NAME") ?? "Test") + ".NET";
    var apiKey = (EnvironmentVariable("PUBLISHER_USERNAME") ?? "") + ":" + (EnvironmentVariable("PUBLISHER_PASSWORD") ?? "");
    foreach (var file in GetFiles($"{csDir}/artifacts/*.nupkg"))
    {
        DotNetCoreTool($"{csDir}", "nuget", "push " + file.FullPath + " --source " + url + " --api-key " + apiKey);
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
