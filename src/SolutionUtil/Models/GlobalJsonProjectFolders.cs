using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace SolutionUtil.Models
{
    public class GlobalJsonProjectFolders : JsonFileItem, IEnumerable<GlobalJsonProjectFolder>
    {
        public GlobalJsonProjectFolders(JToken token) : base(token)
        {
        }

        public IEnumerator<GlobalJsonProjectFolder> GetEnumerator()
        {
            if (Token != null)
            {
                foreach (var folder in Token)
                {
                    yield return new GlobalJsonProjectFolder(folder);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void AddPath(string neededLocation)
        {
            throw new NotImplementedException();
        }
    }
}
