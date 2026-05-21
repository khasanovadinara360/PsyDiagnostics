using System.Collections.Generic;
using Newtonsoft.Json;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Models
{
    public class Question
    {
        public string Text { get; set; } = string.Empty;

        public List<Answer> Answers { get; set; } = new();

        public int Answer { get; set; }

        [JsonIgnore]
        public TestViewModel TestViewModel { get; set; }
    }
}