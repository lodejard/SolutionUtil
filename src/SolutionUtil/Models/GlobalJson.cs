using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SolutionUtil.Models
{
    public class GlobalJson : JsonFileItem
    {
        public GlobalJson(JToken root) : base(root)
        {
        }

        public string FilePath { get; set; }
        public string FolderPath { get; private set; }

        public GlobalJsonProjectFolders ProjectFolders =>
            new GlobalJsonProjectFolders(Token["projects"] ?? Token["sources"]);

        public static GlobalJson Load(string path)
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

            return new GlobalJson(token)
            {
                FilePath = path,
                FolderPath = Path.GetDirectoryName(path),
            };
        }

        public void AddProjectFolder(string neededLocation)
        {
            var array = Token["projects"] ?? Token["sources"];
            if (array == null)
            {
                array = new JArray();
                Token["projects"] = array;
            }
            ((JArray)array).Add(new JValue(neededLocation));
        }

        public void Save()
        {
            using (var file = File.Open(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var textWriter = new StreamWriter(file))
                {
                    using (var jsonWriter = new JsonTextWriter(textWriter))
                    {
                        jsonWriter.Formatting = Formatting.Indented;
                        jsonWriter.Indentation = 2;
                        jsonWriter.IndentChar = ' ';
                        Token.WriteTo(jsonWriter);
                    }
                }
            }
        }
    }
}
