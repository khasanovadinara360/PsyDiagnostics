using Microsoft.ML;
using PsyDiagnostics.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace PsyDiagnostics.Services
{
    public static class ModelTrainer
    {
        public static void Train()
        {
            var ml = new MLContext();

            // путь к файлу
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2.csv");

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл data2.csv не найден");
                return;
            }

            // загрузка данных
            var data = ml.Data.LoadFromTextFile<AiData>(
                path,
                hasHeader: true,
                separatorChar: ',');

            var pipeline = ml.Transforms.Concatenate("Features",
                 nameof(AiData.Aggression),
                 nameof(AiData.Impulsivity),
                 nameof(AiData.Depression),
                 nameof(AiData.Stress),
                 nameof(AiData.Adaptation),
                 nameof(AiData.Anxiety),
                 nameof(AiData.Resilience),
                 nameof(AiData.Hostility))

     // 🔥 ВАЖНО: создаём НОВУЮ колонку LabelBool
             .Append(ml.Transforms.Conversion.ConvertType(
                 outputColumnName: "LabelBool",
                 inputColumnName: nameof(AiData.Label),
                 outputKind: Microsoft.ML.Data.DataKind.Boolean))

     // 🔥 используем LabelBool
               .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
                 labelColumnName: "LabelBool",
                 featureColumnName: "Features"));


            var preview = ml.Data.CreateEnumerable<AiData>(data, false).ToList();
            MessageBox.Show($"Rows before pipeline: {preview.Count}");

            // обучение
            var model = pipeline.Fit(data);

            // сохранение
            ml.Model.Save(model, data.Schema, "model.zip");

            MessageBox.Show("Модель успешно обучена");
        }
    }
}