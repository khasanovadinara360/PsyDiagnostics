using Newtonsoft.Json;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PsyDiagnostics.Services
{
    public static class TestLoader
    {
        private static readonly string FilePath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tests.json");

        public static List<TestDefinition> LoadDefinitions()
        {
            if (!File.Exists(FilePath))
                return new List<TestDefinition>();

            var json = File.ReadAllText(FilePath);

            var definitions = JsonConvert.DeserializeObject<List<TestDefinition>>(json)
                              ?? new List<TestDefinition>();

            return definitions
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        public static List<Test> LoadTests()
        {
            if (!File.Exists(FilePath))
                return new List<Test>();

            var json = File.ReadAllText(FilePath);

            var tests = JsonConvert.DeserializeObject<List<Test>>(json)
                        ?? new List<Test>();

            tests = tests
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .OrderBy(x => x.DisplayName)
                .ToList();

            PrepareTests(tests);

            return tests;
        }

        public static void SaveTests(List<Test> tests)
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(FilePath));

            var json = JsonConvert.SerializeObject(
                tests ?? new List<Test>(),
                Formatting.Indented);

            File.WriteAllText(FilePath, json);
        }

        private static void PrepareTests(IEnumerable<Test> tests)
        {
            foreach (var test in tests)
            {
                foreach (var question in test.Questions)
                {
                    question.TestViewModel = null;

                    foreach (var answer in question.Answers)
                    {
                        answer.Question = question;
                        answer.TestViewModel = null;
                    }
                }
            }
        }
    }
}