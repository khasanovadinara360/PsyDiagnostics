using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.Helpers
{
    public static class JsonHelper
    {
        public static List<Article> LoadArticles()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Data",
                "articles.json");

            if (!File.Exists(path))
                return new List<Article>();

            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<List<Article>>(json);
        }
    }
}