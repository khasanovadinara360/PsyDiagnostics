using Microsoft.ML;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.Services
{
    public class MlService
    {
        private readonly PredictionEngine<AiData, AiPrediction> _engine;
        public MlService()
        {
            var ml = new MLContext();

            var model = ml.Model.Load("model.zip", out _);

            _engine = ml.Model.CreatePredictionEngine<AiData, AiPrediction>(model);
        }

        public AiPrediction Predict(AiData input)
        {
            return _engine.Predict(input);
        }

        public void TrainFromDatabase(DatabaseService db)
        {
        }
    }
}