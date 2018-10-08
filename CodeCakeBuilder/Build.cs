using System.Collections.Generic;
using System.Linq;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Solution;
using Cake.Core;
using Cake.Core.Diagnostics;
using CodeCake;
using SimpleGitVersion;

namespace CodeCake
{
    [AddPath( "%UserProfile%/.nuget/packages/**/tools*" )]
    public partial class Build : CodeCakeHost
    {
        public Build()
        {
            Cake.Log.Verbosity = Verbosity.Diagnostic;

            const string solutionName = "dotnettar";
            const string solutionFileName = solutionName + ".sln";
            
            
            string configuration = Cake.Argument("configuration", "Release");
            IEnumerable<SolutionProject> projects = Cake.ParseSolution( solutionFileName )
                .Projects
                .Where( p => !(p is SolutionFolder)
                             && p.Name != "CodeCakeBuilder" );

            // We do not publish .Tests projects for this solution.
            IEnumerable<SolutionProject> projectsToPublish = projects
                .Where( p => !p.Path.Segments.Contains( "Tests" ) );

            // The SimpleRepositoryInfo should be computed once and only once.
            SimpleRepositoryInfo gitInfo = Cake.GetSimpleRepositoryInfo();
            // This default global info will be replaced by Check-Repository task.
            // It is allocated here to ease debugging and/or manual work on complex build script.


            // Git .ignore file should ignore this folder.
            // Here, we name it "Releases" (default , it could be "Artefacts", "Publish" or anything else, 
            // but "Releases" is by default ignored in https://github.com/github/gitignore/blob/master/VisualStudio.gitignore.
            ConvertableDirectoryPath releasesDir = Cake.Directory("CodeCakeBuilder/Releases");
            CheckRepositoryInfo globalInfo = new CheckRepositoryInfo { Version = gitInfo.SafeNuGetVersion };

            Task("Clean")
                .Does(() =>
               {
                    // Avoids cleaning CodeCakeBuilder itself!
                    Cake.CleanDirectories("**/bin/" + configuration, d => !d.Path.Segments.Contains("CodeCakeBuilder"));
                   Cake.CleanDirectories("**/obj/" + configuration, d => !d.Path.Segments.Contains("CodeCakeBuilder"));
                   Cake.CleanDirectories(releasesDir);
               });
            Task( "Build" )
                .IsDependentOn( "Check-Repository" )
                .IsDependentOn( "Clean" )
                .Does( () =>
                {
                    //StandardSolutionBuild( solutionFileName, gitInfo, globalInfo.BuildConfiguration );
                } );

            Task( "Unit-Testing" )
                .IsDependentOn( "Build" )
                .WithCriteria( () => Cake.InteractiveMode() == InteractiveMode.NoInteraction
                                     || Cake.ReadInteractiveOption( "RunUnitTests", "Run Unit Tests?", 'Y', 'N' ) == 'Y' )
                .Does( () =>
                {
                    //StandardUnitTests( globalInfo.BuildConfiguration, projects.Where( p => p.Name.EndsWith( ".Tests" ) ) );
                } );

        }
    }
}