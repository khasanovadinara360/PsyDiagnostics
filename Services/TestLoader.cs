using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using PsyDiagnostics.Models;

public static class TestLoader
{
    public static List<TestDefinition> LoadAll()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tests.json");

        if (!File.Exists(path))
            return new List<TestDefinition>();

        try
        {
            var json = File.ReadAllText(path);

            var tests = JsonConvert.DeserializeObject<List<Test>>(json);
            if (tests == null)
                return new List<TestDefinition>();

            var defs = new List<TestDefinition>();

            foreach (var t in tests)
            {
                if (string.IsNullOrWhiteSpace(t.Name))
                    continue;

                defs.Add(new TestDefinition
                {
                    Name = t.Name
                    // DisplayName берётся из switch в самом TestDefinition
                });
            }

            return defs;
        }
        catch
        {
            return new List<TestDefinition>();
        }
    }
}