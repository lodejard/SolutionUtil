using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public class GlobalJsonProjectFolder : JsonFileItem
    {
        public GlobalJsonProjectFolder(JToken token) : base(token)
        {
        }

        public string Path => Token.Value<string>();
    }
}
