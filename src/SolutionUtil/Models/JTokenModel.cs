using Newtonsoft.Json.Linq;

namespace SolutionUtil.Models
{
    public abstract class JTokenModel
    {
        protected JTokenModel(JToken token)
        {
            Token = token;
        }

        public JToken Token { get; private set; }
    }
}
