using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using PsyDiagnostics.Models;

public static class TestLoader
{
    public static List<TestDefinition> LoadAll()
    {
        var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");

        if (!Directory.Exists(folder))
            return new List<TestDefinition>();

        var files = Directory.GetFiles(folder, "*.json");

        var tests = new List<TestDefinition>();

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);

            var test = JsonConvert.DeserializeObject<TestDefinition>(json);

            if (test != null)
                tests.Add(test);
        }

        return tests;
    }
}