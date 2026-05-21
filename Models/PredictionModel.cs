namespace PsyDiagnostics.Models
{
    public class PredictionRequest
    {
        public float Aggression { get; set; }

        public float Impulsivity { get; set; }

        public float Depression { get; set; }

        public float Stress { get; set; }

        public float Adaptation { get; set; }

        public float Anxiety { get; set; }

        public float Resilience { get; set; }

        public float Hostility { get; set; }
    }

    public class PredictionResponse
    {
        public int Prediction { get; set; }

        public double Probability { get; set; }
    }
}