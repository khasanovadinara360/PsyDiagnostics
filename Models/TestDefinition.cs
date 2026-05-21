namespace PsyDiagnostics.Models
{
    public class TestDefinition
    {
        public string Name { get; set; } = string.Empty;

        public string DisplayName => GetDisplayName(Name);

        public static string GetDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name switch
            {
                "Aggression" => "Уровень агрессивности",
                "Impulsivity" => "Импульсивность",
                "Depression" => "Депрессивное состояние",
                "Stress" => "Стрессоустойчивость",
                "Adaptation" => "Социальная адаптация",
                "Anxiety" => "Тревожность",
                "Resilience" => "Психологическая устойчивость",
                "Hostility" => "Враждебность",
                _ => FormatCustomName(name)
            };
        }

        private static string FormatCustomName(string name)
        {
            name = name.Replace("_", " ").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}