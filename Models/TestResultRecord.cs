using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsyDiagnostics.Models
{
    public class TestResultRecord
    {
        public string TestName { get; set; }
        public int Score { get; set; }
        public int Prediction { get; set; }
        public string Date { get; set; }
        public double Probability { get; set; }
    }
}