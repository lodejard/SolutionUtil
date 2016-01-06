using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Linq;

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
            using (var file = File.Open(
                            path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read))
            {
                var token = JToken.Load(
                    new JsonTextReader(new StreamReader(file)),
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

            var solutionUtilToken = GetOrAdd(Token, "#solutionUtil", () => new JObject());
            var addedProjects = GetOrAdd(solutionUtilToken, "projects", () => new JArray());
            addedProjects.Add(JValue.CreateString(neededLocation));
        }

        public void ClearAddedProjects()
        {
            var solutionUtilToken = (JObject)Token["#solutionUtil"];
            if (solutionUtilToken == null)
            {
                return;
            }
            try
            {
                var addedPaths = (JArray)solutionUtilToken["projects"];
                if (addedPaths == null)
                {
                    return;
                }
                try
                {
                    var array = (JArray)(Token["projects"] ?? Token["sources"]);
                    foreach (var addedPath in addedPaths)
                    {
                        var path = addedPath.Value<string>();
                        if (array != null)
                        {
                            foreach (var existingPath in array.ToArray())
                            {
                                if (existingPath.Value<string>() == path)
                                {
                                    array.Remove(existingPath);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    solutionUtilToken.Remove("projects");
                }
            }
            finally
            {
                if (!solutionUtilToken.HasValues)
                {
                    ((JObject)Token).Remove("#solutionUtil");
                }
            }
        }


        private T GetOrAdd<T>(JToken token, string name, Func<T> factory) where T : JToken
        {
            var result = (T)token[name];
            if (result == null)
            {
                result = factory();
                token[name] = result;
            }
            return result;
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
