using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public class ProjectJsonDependencies : JTokenModel, IEnumerable<ProjectJsonDependency>
    {
        public ProjectJsonDependencies(JToken token) : base(token)
        {
        }

        public IEnumerator<ProjectJsonDependency> GetEnumerator()
        {
            if (Token != null)
            {
                foreach (var entry in Token)
                {
                    yield return new ProjectJsonDependency(entry);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
