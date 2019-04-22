#tool nuget:?package=codecov
#tool nuget:?package=gitlink
#tool nuget:?package=GitVersion.CommandLine&prerelease
#addin nuget:?package=Cake.Codecov
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#tool "nuget:?package=ReportGenerator&version=4.0.4"

var target = Argument("target", "Build");
var configuration = Argument("Configuration", "Debug");
var version = Argument("NuGetVersion", "");

var libraryProjects = GetFiles("./Libraries/**/*.csproj");
var unitTests = GetFiles("**\\unittest.csproj").Single();
var mockServerTests = GetFiles("**\\dotNetRdf.MockServerTests.csproj").Single();

Task("Pack")
    .IsDependentOn("Build")
    .DoesForEach(libraryProjects, path => {
        var settings = new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = "./nugets/",
            NoBuild = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
        };

        settings.MSBuildSettings.Properties["version"] = new [] { version };

        DotNetCorePack(path.FullPath, settings);
    });

Task("GitVersion")
    .WithCriteria(BuildSystem.IsLocalBuild && string.IsNullOrWhiteSpace(version))
    .Does(() => {
        version = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
        }).NuGetVersion;
    });

Task("Build")
    .IsDependentOn("GitVersion")
    .Does(() => {
        DotNetCoreBuild("dotnetrdf.sln", new DotNetCoreBuildSettings {
            Configuration = configuration
        });
    });

Task("Codecov")
    .Does(() => {
        Codecov("coverage\\cobertura.xml");
    });

Task("TestNet452")
    .IsDependentOn("Build")
    .Does(RunTests(unitTests, "net452"))
    .Does(RunTests(unitTests, "net452", "Category=fulltext"))
    .Does(RunTests(mockServerTests, "net452"))
    .ContinueOnError();

Task("TestNetCore20")
    .IsDependentOn("Build")
    .Does(RunTests(unitTests, "netcoreapp2.0"))
    .ContinueOnError();

Task("TestNetCore11")
    .IsDependentOn("Build")
    .Does(RunTests(unitTests, "netcoreapp1.1"))
    .Does(RunTests(mockServerTests, "netcoreapp1.1"))
    .ContinueOnError();

Task("Test")
    .IsDependentOn("TestNet452")
    .IsDependentOn("TestNetCore20")
    .IsDependentOn("TestNetCore11")
    .Does(() => { });

Task("Cover")
    .IsDependentOn("TestNetCore20")
    .IsDependentOn("TestNetCore11")
    .Does(() => {
        if (DirectoryExists("coverage"))
            CleanDirectories("coverage");
    })
    .Does(RunTests(unitTests, "net452", "Category!=explicit & Coverage=Skip"))
    .Does(DotCover("net452", RunTests(unitTests, "net452", "Category!=explicit & Coverage!=Skip")))
    .Does(DotCover("fulltext", RunTests(unitTests, "net452", "Category=fulltext")))
    .Does(() => {
        DotCoverMerge(GetFiles("coverage\\*.dcvr"), "coverage\\merged.dcvr");
    })
    .Does(() => {        
        DotCoverReport(
          "./coverage/merged.dcvr",
          "./coverage/dotcover.xml",
          new DotCoverReportSettings {
            ReportType = DotCoverReportType.DetailedXML,
          });
    })
    .Does(() => {
        StartProcess(
          @".\tools\ReportGenerator.4.0.4\tools\net47\ReportGenerator.exe",
          @"-reports:.\coverage\dotcover.xml -targetdir:.\coverage -reporttypes:Cobertura -assemblyfilters:-xunit*;-dotNetRDF.*Test");
    })
    .DeferOnError();

public Action<ICakeContext> DotCover(string name, Action<ICakeContext> unitTestRun)
{
    return (ICakeContext c) =>
        DotCoverCover(
            unitTestRun,
            $"./coverage/{name}.dcvr",
            new DotCoverCoverSettings());
}

public Action<ICakeContext> RunTests(FilePath project, string framework, string filter = "Category!=explicit")
{
    return (ICakeContext ctx) => 
        ctx.DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings
        {
             Configuration = configuration,
             Framework = framework,
             NoBuild = true,
             Filter = filter
        });
}

RunTarget(target);
