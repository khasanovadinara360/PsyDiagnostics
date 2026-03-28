using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.Models
{
    public class TestDefinition
    {
        public string Name { get; set; } // ID (из JSON)

        public string DisplayName => Name switch
        {
            "Aggression" => "Уровень агрессивности",
            "Impulsivity" => "Импульсивность",
            "Depression" => "Депрессивное состояние",
            "Stress" => "Стрессоустойчивость",
            "Adaptation" => "Социальная адаптация",
            "Anxiety" => "Тревожность",
            "Resilience" => "Психологическая устойчивость",
            "Hostility" => "Враждебность",
            _ => Name
        };
    }
}