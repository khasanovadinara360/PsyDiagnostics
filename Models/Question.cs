using System.Collections.Generic;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Models
{
    public class Question
    {
        public string Text { get; set; }
        public List<Answer> Answers { get; set; } = new();
        public int Answer { get; set; }
        public TestViewModel TestViewModel { get; set; }
    }
}