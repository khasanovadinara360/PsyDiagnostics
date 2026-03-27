using Microsoft.ML;
using PsyDiagnostics.Models;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class MLPredictor
    {
        private PredictionEngine<RiskData, RiskPrediction> engine;

        public MLPredictor()
        {
            var ml = new MLContext();

            var model = ml.Model.Load("model.zip", out _);

            engine = ml.Model.CreatePredictionEngine<RiskData, RiskPrediction>(model);
        }

        public string Predict(Dictionary<string, int> r)
        {
            var input = new RiskData
            {
                Aggression = r.GetValueOrDefault("Агрессивность"),
                Impulsivity = r.GetValueOrDefault("Импульсивность"),
                Stress = r.GetValueOrDefault("Стресс"),
                Adaptation = r.GetValueOrDefault("Социальная адаптация"),
                Emotional = r.GetValueOrDefault("Эмоциональное состояние")
            };

            return engine.Predict(input).Prediction;
        }
    }
}