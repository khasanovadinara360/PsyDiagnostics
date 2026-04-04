using Newtonsoft.Json;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PsyDiagnostics.Services
{
    public class TestService
    {
        public List<Test> LoadTests()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tests.json");
            var json = File.ReadAllText(path);
            var tests = JsonConvert.DeserializeObject<List<Test>>(json);

            foreach (var test in tests)
            {
                foreach (var question in test.Questions)
                {
                    question.TestViewModel = null; // будет привязан позже

                    foreach (var answer in question.Answers)
                    {
                        answer.Question = question;
                        answer.TestViewModel = null;
                    }
                }
            }

            return tests;
        }
    }
}