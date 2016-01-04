using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SolutionUtil.Models
{
    public class ProjectJson : JsonFileItem
    {
        public ProjectJson(JToken token) : base(token)
        {
        }

        public string FilePath { get; private set; }
        public string FolderPath { get; private set; }
        public string ParentPath => Path.GetDirectoryName(FolderPath);

        public ProjectJsonDependencies Dependencies =>
            new ProjectJsonDependencies(Token["dependencies"]);

        public static ProjectJson Load(string path)
        {
            var token = JToken.Load(
                new JsonTextReader(
                    new StreamReader(
                        File.Open(
                            path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read))),
                new JsonLoadSettings
                {
                    LineInfoHandling = LineInfoHandling.Load,
                    CommentHandling = CommentHandling.Load
                });

            return new ProjectJson(token)
            {
                FilePath = path,
                FolderPath = Path.GetDirectoryName(path),
            };
        }
    }
}