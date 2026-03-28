using System.Collections.Generic;

namespace PsyDiagnostics.Models
{
    public class Question
    {
        public string Text { get; set; }

        public List<Answer> Answers { get; set; } = new();

        public int Answer { get; set; } // выбранный ответ
    }
}