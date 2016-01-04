using System;
using System.Collections.Generic;
using System.IO;

namespace SolutionUtil.Models
{
    public class DotnetProject
    {
        public ProjectJson ProjectJson { get; private set; }

        public string Name => Path.GetFileName(ProjectJson.FolderPath);

        public DotnetSolution Solution { get; set; }

        public List<DotnetProjectDependency> Dependencies { get; private set; } = new List<DotnetProjectDependency>();

        public static DotnetProject Load(string projectJsonPath)
        {
            return new DotnetProject
            {
                ProjectJson = ProjectJson.Load(projectJsonPath),
            };
        }
    }
}
