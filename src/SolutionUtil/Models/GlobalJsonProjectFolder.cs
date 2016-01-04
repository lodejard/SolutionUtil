using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public class GlobalJsonProjectFolder : JTokenModel
    {
        public GlobalJsonProjectFolder(JToken token) : base(token)
        {
        }

        public string Path => Token.Value<string>();
    }
}
