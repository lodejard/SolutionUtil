using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionUtil.Models
{
    public class DotnetSolution
    {
        public GlobalJson GlobalJson { get; private set; }
        public List<DotnetProject> Projects { get; internal set; } = new List<DotnetProject>();

        public static DotnetSolution Load(string globalJsonPath)
        {
            return new DotnetSolution
            {
                GlobalJson = GlobalJson.Load(globalJsonPath),
            };
        }
    }
}
