using QuestPDF.Fluent;
using QuestPDF.Helpers;
using PsyDiagnostics.Models;
using System.Collections.Generic;

namespace PsyDiagnostics.Services
{
    public class ReportService
    {
        public void Generate(string file, Participant p, List<ResultRecord> res, string risk)
        {
            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Margin(30);

                    page.Content().Column(col =>
                    {
                        col.Item().Text("ПСИХОЛОГИЧЕСКИЙ ОТЧЁТ").FontSize(20).Bold();

                        col.Item().Text($"ФИО: {p.Name}");
                        col.Item().Text($"Возраст: {p.Age}");

                        foreach (var r in res)
                            col.Item().Text($"{r.TestName}: {r.Score} ({r.Date})");

                        col.Item().Text($"Риск: {risk}")
                                  .FontColor(Colors.Red.Medium)
                                  .Bold();
                    });
                });
            }).GeneratePdf(file);
        }
    }
}