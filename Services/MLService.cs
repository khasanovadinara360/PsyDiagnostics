using Microsoft.ML;
using PsyDiagnostics.Models;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class MLService
    {
        public void Train()
        {
            var ml = new MLContext();

            var data = new List<RiskData>
            {
                new RiskData{Aggression=5,Impulsivity=5,Stress=5,Adaptation=25,Emotional=5,Label="Low"},
                new RiskData{Aggression=20,Impulsivity=20,Stress=15,Adaptation=10,Emotional=20,Label="High"},
                new RiskData{Aggression=12,Impulsivity=10,Stress=10,Adaptation=15,Emotional=12,Label="Medium"}
            };

            var dv = ml.Data.LoadFromEnumerable(data);

            var pipeline =
                ml.Transforms.Conversion.MapValueToKey("Label")
                .Append(ml.Transforms.Concatenate("Features",
                    nameof(RiskData.Aggression),
                    nameof(RiskData.Impulsivity),
                    nameof(RiskData.Stress),
                    nameof(RiskData.Adaptation),
                    nameof(RiskData.Emotional)))
                .Append(ml.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dv);

            ml.Model.Save(model, dv.Schema, "model.zip");
        }
    }
}