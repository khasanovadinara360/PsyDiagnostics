using System.Collections.Generic;

namespace PsyDiagnostics.Models
{
    public class Test
    {
        public string Name { get; set; } = string.Empty;

        public int LowMax { get; set; }

        public int MediumMax { get; set; }

        public List<Question> Questions { get; set; } = new();

        public string DisplayName => TestDefinition.GetDisplayName(Name);
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