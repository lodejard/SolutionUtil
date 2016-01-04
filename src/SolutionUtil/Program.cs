using System;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using SolutionUtil.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolutionUtil
{
    public class Program
    {
        public int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Command("crossref", cmdCrossref =>
            {
                cmdCrossref.Command("add", cmdCrossrefAdd =>
                {
                    var argPath = cmdCrossrefAdd.Argument("path", "Root path to scan for global.json files");
                    cmdCrossrefAdd.OnExecute(() =>
                        OnCrossrefAdd(cmdCrossrefAdd, argPath));
                });
            });
            return app.Execute(args);
        }

        private int OnCrossrefAdd(
            CommandLineApplication cmd,
            CommandArgument argPath)
        {
            var path = argPath.Value;

            Console.WriteLine($"Executing: Crossref Add {path}");

            var operations = new WorkspaceOperations();

            var workspace = new DotnetWorkspace();

            operations.FindAndLoadSolutions(workspace, path);

            operations.FindAndLoadProjects2(workspace);

            operations.FindProjectToProjectDependencies(workspace);

            foreach (var solution in workspace.Solutions)
            {
                var allDependencies = solution
                    .Projects
                    .SelectMany(project => project.Dependencies.Select(dependency => new { project, dependency }));

                var p2pLocations = allDependencies
                    .Where(x => x.project.ProjectJson.ParentPath != x.dependency.DotnetProject.ProjectJson.ParentPath)
                    .GroupBy(x => MakeRelativePath(solution.GlobalJson.FilePath, x.dependency.DotnetProject.ProjectJson.ParentPath).Replace("\\", "/"))
                    .ToArray();

                var neededLocations = new List<string>();

                Console.WriteLine($"  {solution.GlobalJson.FolderPath}");
                foreach (var p2pLocation in p2pLocations)
                {
                    neededLocations.Add(p2pLocation.Key);
                    Console.WriteLine($"    {p2pLocation.Key}");
                    foreach (var dependencyName in p2pLocation.Select(x => x.dependency.DotnetProject.Name).Distinct())
                    {
                        Console.WriteLine($"      {dependencyName}");
                    }
                }

                foreach (var neededLocation in neededLocations)
                {
                    if (!solution.GlobalJson.ProjectFolders.Any(pf =>
                        string.Equals(pf.Path, neededLocation, StringComparison.OrdinalIgnoreCase)))
                    {
                        solution.GlobalJson.AddProjectFolder(neededLocation);
                    }
                }
            }

            foreach (var solution in workspace.Solutions)
            {
                //solution.GlobalJson.FilePath += ".txt";
                solution.GlobalJson.Save();
            }
            return 0;
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
