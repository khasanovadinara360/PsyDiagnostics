using Microsoft.ML.Data;

namespace PsyDiagnostics.Models
{
    public class RiskPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }
    }
}