using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dnx.Runtime.Common.CommandLine;
using SolutionUtil.Models;

namespace SolutionUtil
{
    public class Program
    {
        public int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Command("crossref", cmd =>
            {
                cmd.Command("add", subcmd =>
                {
                    var argPath = subcmd.Argument("path", "Root path to scan for global.json files");
                    subcmd.OnExecute(() =>
                        OnCrossrefAdd(subcmd, argPath));
                });
                cmd.Command("clear", subcmd =>
                {
                    var argPath = subcmd.Argument("path", "Root path to scan for global.json files");
                    subcmd.OnExecute(() =>
                        OnCrossrefClear(subcmd, argPath));
                });
            });
            return app.Execute(args);
        }


        private int OnCrossrefAdd(
            CommandLineApplication cmd,
            CommandArgument argPath)
        {
            var paths = new[] { argPath.Value }
                .Concat(cmd.RemainingArguments)
                .Select(path => Path.GetFullPath(path))
                .ToArray();

            Console.WriteLine($"Executing Crossref Add");

            var operations = new WorkspaceOperations();
            var workspace = new DotnetWorkspace();

            foreach (var path in paths)
            {
                operations.FindAndLoadSolutions(workspace, path);
            }
            operations.FindAndLoadProjects2(workspace);
            operations.FindProjectToProjectDependencies(workspace);

            foreach (var solution in workspace.Solutions)
            {
                var allProject = solution.Projects.Select(p => Tuple.Create(p, p, p));

                var allProject2Project = SelectRecursive(0, allProject);

                var allProject2ProjectLocations = allProject2Project
                    .Where(tuple => tuple.Item1.ProjectJson.ParentPath != tuple.Item3.ProjectJson.ParentPath)
                    .GroupBy(tuple => MakeRelativePath(solution.GlobalJson.FilePath, tuple.Item3.ProjectJson.ParentPath).Replace("\\", "/"))
                    .ToArray();

                var neededLocations = new List<string>();

//                Console.WriteLine($"  {solution.GlobalJson.FolderPath}");
                foreach (var project2ProjectLocation in allProject2ProjectLocations)
                {
                    neededLocations.Add(project2ProjectLocation.Key);
  //                  Console.WriteLine($"    {project2ProjectLocation.Key}");
                    foreach (var dependencyName in project2ProjectLocation.Select(x => x.Item3.Name).Distinct())
                    {
    //                    Console.WriteLine($"      {dependencyName}");
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
                solution.GlobalJson.Save();
            }

            using (var batch = File.CreateText("restore-all.cmd"))
            {
                foreach (var p in workspace.Projects)
                {
                    batch.WriteLine($"call dnu restore {p.ProjectJson.FolderPath}");
                }
            }
            return 0;
        }

        private int OnCrossrefClear(CommandLineApplication cmd, CommandArgument argPath)
        {
            var paths = new[] { argPath.Value }
                .Concat(cmd.RemainingArguments)
                .Select(path => Path.GetFullPath(path))
                .ToArray();

            Console.WriteLine($"Executing Crossref Clear");

            var operations = new WorkspaceOperations();
            var workspace = new DotnetWorkspace();

            foreach (var path in paths)
            {
                operations.FindAndLoadSolutions(workspace, path);
            }

            foreach (var solution in workspace.Solutions)
            {
                solution.GlobalJson.ClearAddedProjects();
            }

            foreach (var solution in workspace.Solutions)
            {
                solution.GlobalJson.Save();
            }

            return 0;
        }

        private IEnumerable<Tuple<DotnetProject, DotnetProject, DotnetProject>> SelectRecursive(
            int depth,
            IEnumerable<Tuple<DotnetProject, DotnetProject, DotnetProject>> projects)
        {
            if (depth == 64)
            {
                int x = 5;
            }

            var dependencies = projects
                .SelectMany(tuple => tuple.Item3.Dependencies.Select(d => Tuple.Create(tuple.Item1, tuple.Item3, d.DotnetProject)))
                .Where(tuple => tuple.Item3 != null);

            if (dependencies.Any())
            {
                return projects.Concat(SelectRecursive(depth + 1, dependencies)).Distinct();
            }
            return projects;
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
