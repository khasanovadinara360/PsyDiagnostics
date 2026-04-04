using System.Collections.Generic;

namespace PsyDiagnostics.Models
{
    public class Test
    {
        public string Name { get; set; }
        public int LowMax { get; set; }
        public int MediumMax { get; set; }
        public List<Question> Questions { get; set; } = new();

        // Русское отображаемое название, по тому же правилу, что и TestDefinition
        public string DisplayName => new TestDefinition { Name = Name }.DisplayName;

        public string GetLevel(int score)
        {
            if (score <= LowMax)
                return "Низкий уровень";

            if (score <= MediumMax)
                return "Средний уровень";

            return "Высокий уровень";
        }
    }
}