using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public class ProjectJsonDependency : JsonFileItem
    {
        public ProjectJsonDependency(JToken token) : base(token)
        {
        }

        public string Name => (Token as JProperty)?.Name;
    }
}
