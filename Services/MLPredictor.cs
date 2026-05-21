using Microsoft.ML;
using PsyDiagnostics.Models;
using System.Collections.Generic;
using System.IO;

namespace PsyDiagnostics.Services
{
    public class MLPredictor
    {
        private readonly PredictionEngine<AiData, AiPrediction> _engine;

        public MLPredictor()
        {
            var ml = new MLContext();

            var modelPath = Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "model.zip");

            var model = ml.Model.Load(modelPath, out _);

            _engine = ml.Model.CreatePredictionEngine<AiData, AiPrediction>(model);
        }

        public string Predict(Dictionary<string, int> results)
        {
            var input = new AiData
            {
                Aggression = results.GetValueOrDefault("Агрессивность") / 30f,
                Impulsivity = results.GetValueOrDefault("Импульсивность") / 30f,
                Stress = results.GetValueOrDefault("Стресс") / 30f,
                Adaptation = results.GetValueOrDefault("Социальная адаптация") / 30f,
                Depression = results.GetValueOrDefault("Эмоциональное состояние") / 30f,

                Anxiety = 0.5f,
                Resilience = 0.5f,
                Hostility = 0.5f
            };

            var result = _engine.Predict(input);

            return result.Prediction
                ? "Высокий риск"
                : "Низкий риск";
        }
    }
}