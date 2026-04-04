using Microsoft.ML;
using PsyDiagnostics.Models;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class MLPredictor
    {
        private readonly PredictionEngine<AiData, AiPrediction> engine;

        public MLPredictor()
        {
            var ml = new MLContext();
            var model = ml.Model.Load("model.zip", out _);

            engine = ml.Model.CreatePredictionEngine<AiData, AiPrediction>(model);
        }

        public string Predict(Dictionary<string, int> r)
        {
            var input = new AiData
            {
                Aggression = r.GetValueOrDefault("Агрессивность") / 30f,
                Impulsivity = r.GetValueOrDefault("Импульсивность") / 30f,
                Stress = r.GetValueOrDefault("Стресс") / 30f,
                Adaptation = r.GetValueOrDefault("Социальная адаптация") / 30f,
                Depression = r.GetValueOrDefault("Эмоциональное состояние") / 30f,

                Anxiety = 0.5f,
                Resilience = 0.5f,
                Hostility = 0.5f
            };

            var result = engine.Predict(input);

            return result.Prediction ? "🔴 Высокий риск" : "🟢 Низкий риск";
        }
    }
}