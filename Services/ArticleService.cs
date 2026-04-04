using Newtonsoft.Json;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PsyDiagnostics.Services
{
    public class ArticleService
    {
        public List<Article> Load()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "articles.json");
            if (!File.Exists(path))
                return new List<Article>();

            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<List<Article>>(json) ?? new List<Article>();
        }
    }
}