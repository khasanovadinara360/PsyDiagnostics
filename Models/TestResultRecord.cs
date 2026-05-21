namespace PsyDiagnostics.Models
{
    public class TestResultRecord
    {
        public string TestName { get; set; } = string.Empty;

        public int Score { get; set; }

        public int Prediction { get; set; }

        public double Probability { get; set; }

        public double RiskScore { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;
    }
}