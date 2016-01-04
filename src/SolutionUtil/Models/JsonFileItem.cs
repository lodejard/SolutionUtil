using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public abstract class JsonFileItem
    {
        protected JsonFileItem(JToken token)
        {
            Token = token;
        }

        public JToken Token { get; private set; }
    }
}
