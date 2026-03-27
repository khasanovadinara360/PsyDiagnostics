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
            return JsonConvert.DeserializeObject<List<Test>>(json);

        }
    }
}