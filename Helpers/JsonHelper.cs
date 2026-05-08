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
        // =========================
        // СТАТЬИ
        // =========================

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

        // =========================
        // ТЕСТЫ
        // =========================

        public static List<Test> LoadTests()
        {
            var tests = new List<Test>();

            var folders = new[]
            {
        Path.Combine(Directory.GetCurrentDirectory(), "Tests"),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests"),
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Tests"))
    };

            foreach (var folder in folders.Distinct())
            {
                if (!Directory.Exists(folder))
                    continue;

                foreach (var file in Directory.GetFiles(folder, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var test = JsonConvert.DeserializeObject<Test>(json);

                        if (test != null && !tests.Any(x => x.Name == test.Name))
                            tests.Add(test);
                    }
                    catch { }
                }
            }

            return tests.OrderBy(t => t.Name).ToList();
        }

        public static void SaveTest(Test test)
        {
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Tests");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{test.Name}.json";

            var path = Path.Combine(folder, fileName);

            var json = JsonConvert.SerializeObject(
                test,
                Formatting.Indented);

            File.WriteAllText(path, json);
        }
    }
}