using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolutionUtil.Models
{
    public class DotnetWorkspace
    {
        public List<DotnetSolution> Solutions { get; private set; } = new List<DotnetSolution>();

        public IEnumerable<DotnetProject> Projects => Solutions.SelectMany(s => s.Projects);
    }
}
