using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsyDiagnostics.Models
{
    public class RiskData
    {
        public float Aggression { get; set; }
        public float Impulsivity { get; set; }
        public float Stress { get; set; }
        public float Adaptation { get; set; }
        public float Emotional { get; set; }

        public string Label { get; set; }
    }
}