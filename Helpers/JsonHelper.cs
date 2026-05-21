using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.Helpers
{
    public static class JsonHelper
    {
        private static string DataFolder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        public static List<Article> LoadArticles()
        {
            var path = Path.Combine(DataFolder, "articles.json");

            if (!File.Exists(path))
                return new List<Article>();

            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<List<Article>>(json)
                   ?? new List<Article>();
        }

        public static List<Test> LoadTests()
        {
            var path = Path.Combine(DataFolder, "tests.json");

            if (!File.Exists(path))
                return new List<Test>();

            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<List<Test>>(json)
                       ?.Where(t => t != null && !string.IsNullOrWhiteSpace(t.Name))
                       .GroupBy(t => t.Name)
                       .Select(g => g.First())
                       .OrderBy(t => t.Name)
                       .ToList()
                   ?? new List<Test>();
        }

        public static void SaveTest(Test test)
        {
            if (test == null || string.IsNullOrWhiteSpace(test.Name))
                return;

            Directory.CreateDirectory(DataFolder);

            var path = Path.Combine(DataFolder, "tests.json");

            var tests = LoadTests();
            var existing = tests.FirstOrDefault(x => x.Name == test.Name);

            if (existing != null)
                tests.Remove(existing);

            tests.Add(test);

            var json = JsonConvert.SerializeObject(
                tests.OrderBy(t => t.Name).ToList(),
                Formatting.Indented);

            File.WriteAllText(path, json);
        }
    }
}