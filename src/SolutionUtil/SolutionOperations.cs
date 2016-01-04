using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using SolutionUtil.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionUtil
{
    public class WorkspaceOperations
    {
        public void FindAndLoadSolutions(DotnetWorkspace workspace, string path)
        {
            var globalJsonMatches = new Matcher()
                .AddInclude("**/global.json")
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(path)));

            foreach (var file in globalJsonMatches.Files)
            {
                var globalJsonPath = Path.Combine(path, file.Path);

                Console.WriteLine($"Loading {globalJsonPath}");

                workspace.Solutions.Add(DotnetSolution.Load(globalJsonPath));
            }
        }

        public void FindAndLoadProjects1(DotnetWorkspace workspace)
        {
            foreach (var solution in workspace.Solutions)
            {
                if (!solution.GlobalJson.ProjectFolders.Any())
                {
                    Console.WriteLine($"NO 'projects' folders {solution.GlobalJson.FolderPath}");
                    continue;
                }
                foreach (var projectFolder in solution.GlobalJson.ProjectFolders)
                {
                    var scanPath = Path.Combine(solution.GlobalJson.FolderPath, projectFolder.Path);

                    if (!scanPath.StartsWith(solution.GlobalJson.FolderPath) ||
                        projectFolder.Path.StartsWith("."))
                    {
                        Console.WriteLine($"OUTSIDE FOLDER {solution.GlobalJson.FolderPath}:{projectFolder.Path}");
                        continue;
                    }

                    Console.WriteLine($"Scanning {solution.GlobalJson.FolderPath}:{projectFolder.Path}");

                    var projectJsonMatches = new Matcher()
                        .AddInclude("*/project.json")
                        .Execute(new DirectoryInfoWrapper(new DirectoryInfo(scanPath)));

                    foreach (var file in projectJsonMatches.Files)
                    {
                        var projectJsonPath = Path.Combine(scanPath, file.Path);
                        Console.WriteLine($"  Loading {projectJsonPath}");

                        var project = DotnetProject.Load(projectJsonPath);
                        project.Solution = solution;
                        solution.Projects.Add(project);
                    }
                }
            }
        }

        public void FindAndLoadProjects2(DotnetWorkspace workspace)
        {
            foreach (var solution in workspace.Solutions)
            {
                var scanPath = solution.GlobalJson.FolderPath;

                var projectJsonMatches = new Matcher()
                    .AddInclude("**/project.json")
                    .Execute(new DirectoryInfoWrapper(new DirectoryInfo(scanPath)));

                foreach (var file in projectJsonMatches.Files)
                {
                    var projectJsonPath = Path.Combine(scanPath, file.Path);
                    Console.WriteLine($"  Loading {projectJsonPath}");

                    var project = DotnetProject.Load(projectJsonPath);
                    project.Solution = solution;
                    solution.Projects.Add(project);
                }
            }
        }

        public void FindProjectToProjectDependencies(DotnetWorkspace workspace)
        {
            foreach (var project in workspace.Projects)
            {
                foreach (var dependency in project.ProjectJson.Dependencies)
                {
                    var match = workspace.Projects.SingleOrDefault(p =>
                                string.Equals(p.Name, dependency.Name, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        project.Dependencies.Add(new DotnetProjectDependency
                        {
                            ProjectJsonDependency = dependency,
                            DotnetProject = match,
                        });
                    }
                }
            }
        }
    }
}
