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
                Aggression = results.GetValueOrDefault("Уровень агрессивности") / 30f,
                Impulsivity = results.GetValueOrDefault("Импульсивность") / 30f,
                Depression = results.GetValueOrDefault("Депрессивное состояние") / 30f,
                Stress = results.GetValueOrDefault("Стрессоустойчивость") / 30f,
                Adaptation = results.GetValueOrDefault("Социальная адаптация") / 30f,

                Anxiety = results.GetValueOrDefault("Тревожность") / 30f,
                Resilience = results.GetValueOrDefault("Психологическая устойчивость") / 30f,
                Hostility = results.GetValueOrDefault("Враждебность") / 30f
            };

            var result = _engine.Predict(input);

            return result.Prediction
                ? "Высокий риск"
                : "Низкий риск";
        }
    }
}