using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class RiskService
    {
        public string CalculateRisk(Dictionary<string, int> r)
        {
            int risk = 0;

            risk += r.GetValueOrDefault("Агрессивность") * 2;
            risk += r.GetValueOrDefault("Импульсивность") * 2;
            risk += r.GetValueOrDefault("Стресс");

            risk += (30 - r.GetValueOrDefault("Социальная адаптация")) * 2;

            if (risk < 60) return "Низкий риск";
            if (risk < 120) return "Средний риск";
            return "Высокий риск";
        }
    }
}