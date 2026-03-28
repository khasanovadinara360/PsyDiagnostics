using Microsoft.ML.Data;

namespace PsyDiagnostics.Models
{
    public class AiData
    {
        [LoadColumn(0)] public float Aggression { get; set; }
        [LoadColumn(1)] public float Impulsivity { get; set; }
        [LoadColumn(2)] public float Depression { get; set; }
        [LoadColumn(3)] public float Stress { get; set; }
        [LoadColumn(4)] public float Adaptation { get; set; }
        [LoadColumn(5)] public float Anxiety { get; set; }
        [LoadColumn(6)] public float Resilience { get; set; }
        [LoadColumn(7)] public float Hostility { get; set; }

        [LoadColumn(8)] public float Label { get; set; }
    }

    public class AiPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }
        public float Score { get; set; }
    }
}