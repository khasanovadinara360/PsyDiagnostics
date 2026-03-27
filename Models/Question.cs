namespace PsyDiagnostics.Models
{
    public class Question
    {
        public string Text { get; set; }

        // ответ пользователя (0–3)
        public int Answer { get; set; }
    }
}