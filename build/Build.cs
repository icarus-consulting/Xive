using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.OpenCover;
using Nuke.Common.Utilities.Collections;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.OpenCover.OpenCoverTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.FullBuild);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [NuGetPackage("CodecovUploader", "codecov.exe")]
    readonly Tool CodecovUploader;

    [GitRepository] readonly GitRepository GitRepository;
    AbsolutePath RepositoryDirectory => BuildProjectDirectory / "..";
    AbsolutePath SourceDirectory => RepositoryDirectory / "src";
    AbsolutePath TestsDirectory => RepositoryDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RepositoryDirectory / "artifacts";
    AbsolutePath SolutionFile => RepositoryDirectory / "Xive.sln";
    AbsolutePath XiveProjectFile => RepositoryDirectory / "src" / "Xive" / "Xive.csproj";
    AbsolutePath TestProjectFile => RepositoryDirectory / "tests" / "Test.Xive" / "Test.Xive.csproj";

    private string NuGetFeed => "https://api.nuget.org/v3/index.json";
    private string NUGET_TOKEN = Environment.GetEnvironmentVariable("NUGET_TOKEN");

    private AbsolutePath CoverageFile => ArtifactsDirectory / "coverage.xml";
    private string CODECOV_TOKEN = Environment.GetEnvironmentVariable("CODECOV_TOKEN");

    private Version Version;

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(SolutionFile));
        });

    Target DefaultVersion => _ => _
       .Executes(() =>
       {
           var document = XDocument.Load(XiveProjectFile);
           Version = Version.Parse(document.Root?.Element("PropertyGroup")?.Element("Version")?.Value ?? throw new InvalidOperationException("Version property missing in Xive.csproj"));
       });


    Target VersionFromTag => _ => _
        .DependsOn(DefaultVersion)
        .OnlyWhenDynamic(() => IsServerBuild && AppVeyor.Instance.RepositoryTag)
        .Executes(() =>
        {
            Version = Version.Parse(AppVeyor.Instance.RepositoryTagName);
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(VersionFromTag)
        .Executes(() =>
        {
            Console.WriteLine($"Using Version '{Version}'");
            DotNetBuild(s => s
                .SetProjectFile(SolutionFile)
                .SetConfiguration(Configuration)
                .SetVersion(Version.ToString())
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(TestProjectFile)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetConfiguration(Configuration)
            );
        });


    Target CreateCoverageReport => _ => _
        .Executes(() =>
        {
            OpenCover(s => s
                .SetOutput(CoverageFile)
                .SetTargetSettings(
                    new DotNetTestSettings()
                        .SetProjectFile(TestProjectFile)
                        .SetConfiguration("Debug")
                )
                .SetFilters("+[Xive]*")
                .SetOldStyle(true)
            );
        });


    Target CodeCove => _ => _
        .DependsOn(CreateCoverageReport)
        .OnlyWhenDynamic(() => IsServerBuild
            && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            && !string.IsNullOrWhiteSpace(CODECOV_TOKEN))
        .Executes(() =>
        {
            Console.WriteLine($"CoverageFile is Empty = {string.IsNullOrEmpty(CoverageFile)}");
            Console.WriteLine($"CODECOV_TOKEN is Empty = {string.IsNullOrEmpty(CODECOV_TOKEN)}");

            CodecovUploader($"-f {CoverageFile} -t {CODECOV_TOKEN}");
        });


    Target Pack => _ => _
        .DependsOn(Compile)
        .DependsOn(VersionFromTag)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(XiveProjectFile)
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
                .SetVersion(Version.ToString())
                .EnableIncludeSymbols()
                .SetOutputDirectory(ArtifactsDirectory)
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
            );
        });

    Target PushPackage => _ => _
        .OnlyWhenDynamic(() => IsServerBuild && AppVeyor.Instance.RepositoryTag && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        .DependsOn(Pack)
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(NuGetFeed)
                .SetApiKey(NUGET_TOKEN)
                .CombineWith(ArtifactsDirectory.GlobFiles("*.nupkg", "*.snupkg"), (_, v) => _
                    .SetTargetPath(v)
                ),
                degreeOfParallelism: 2,
                completeOnFailure: false
            );
        });

    Target FullBuild => _ => _
        .DependsOn(Compile)
        .DependsOn(CodeCove)
        .DependsOn(PushPackage)
        .Executes(() =>
        {

        });
}
