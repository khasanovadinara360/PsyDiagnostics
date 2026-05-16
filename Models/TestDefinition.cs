using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsyDiagnostics.Models
{
    public class TestDefinition
    {
        public string Name { get; set; }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return "";

                return Name switch
                {
                    "Aggression" => "Уровень агрессивности",
                    "Impulsivity" => "Импульсивность",
                    "Depression" => "Депрессивное состояние",
                    "Stress" => "Стрессоустойчивость",
                    "Adaptation" => "Социальная адаптация",
                    "Anxiety" => "Тревожность",
                    "Resilience" => "Психологическая устойчивость",
                    "Hostility" => "Враждебность",

                    // для всех новых тестов
                    _ => FormatCustomName(Name)
                };
            }
        }

        private string FormatCustomName(string name)
        {
            name = name.Replace("_", " ").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return "";

            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}