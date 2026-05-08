using Microsoft.Win32;
using PsyDiagnostics.Models;
using PsyDiagnostics.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PsyDiagnostics.Services
{
    public static class PdfReportService
    {
        public static void GenerateTestingReport(
            Participant current,
            IEnumerable<TestHistoryItem> testHistory,
            string unitRisk)
        {
            if (current == null)
            {
                MessageBox.Show("Нет участника");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Отчет_{GetSafeFileName(current.FullName)}_{DateTime.Now:dd.MM.yyyy}.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            QuestPDF.Settings.License = LicenseType.Community;

            var tests = testHistory?.ToList() ?? new List<TestHistoryItem>();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Психодиагностический отчет")
                            .FontSize(22)
                            .Bold();

                        header.Item().Text("Раздел: тестирование")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text("Данные обследуемого")
                            .FontSize(16)
                            .Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            InfoRow(table, "ФИО", current.FullName);
                            InfoRow(table, "ID", current.PrisonerId.ToString());
                            InfoRow(table, "Отряд", current.Unit);
                            InfoRow(table, "Дата отчета", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                            InfoRow(table, "Риск по отряду", unitRisk);
                        });

                        col.Item().PaddingTop(10).Text("Пройденные тесты")
                            .FontSize(16)
                            .Bold();

                        if (tests.Count == 0)
                        {
                            col.Item().Text("По выбранному участнику нет сохраненных результатов тестирования.");
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("Тест").Bold();
                                    header.Cell().Element(HeaderCell).Text("Баллы").Bold();
                                    header.Cell().Element(HeaderCell).Text("Риск").Bold();
                                    header.Cell().Element(HeaderCell).Text("Дата").Bold();
                                });

                                foreach (var item in tests)
                                {
                                    table.Cell().Element(Cell).Text(item.TestName ?? "");
                                    table.Cell().Element(Cell).Text(item.Score.ToString());
                                    table.Cell().Element(Cell).Text(item.Risk ?? "");
                                    table.Cell().Element(Cell).Text(item.Date ?? "");
                                }
                            });
                        }

                        col.Item().PaddingTop(10).Text("Заключение и прогноз")
                            .FontSize(16)
                            .Bold();

                        col.Item().Text(BuildSummary(tests));
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("PsyDiagnostics • страница ");
                            text.CurrentPageNumber();
                        });
                });
            }).GeneratePdf(dialog.FileName);

            MessageBox.Show("PDF-отчет успешно сохранен");
        }

        private static void InfoRow(TableDescriptor table, string title, string value)
        {
            table.Cell().Element(HeaderCell).Text(title).Bold();
            table.Cell().Element(Cell).Text(string.IsNullOrWhiteSpace(value) ? "—" : value);
        }

        private static QuestPDF.Infrastructure.IContainer Cell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }

        private static QuestPDF.Infrastructure.IContainer HeaderCell(
            QuestPDF.Infrastructure.IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }

        private static string BuildSummary(List<TestHistoryItem> tests)
        {
            if (tests == null || tests.Count == 0)
                return "Тестирование не проводилось. Для формирования заключения необходимо пройти хотя бы один тест.";

            double avg = tests.Average(x => x.Score);

            string level;
            if (avg <= 32)
                level = "низкий";
            else if (avg <= 66)
                level = "средний";
            else
                level = "высокий";

            var maxItem = tests.OrderByDescending(x => x.Score).FirstOrDefault();

            return
                $"Общий прогноз риска: {level}. Средний показатель по пройденным тестам составляет {avg:F1}%. " +
                $"Наиболее выраженный показатель: {maxItem?.TestName} — {maxItem?.Score} баллов. " +
                "Рекомендуется учитывать результаты тестирования при дальнейшем психологическом сопровождении, " +
                "проводить наблюдение в динамике и при необходимости назначать индивидуальную коррекционную работу.";
        }

        private static string GetSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Без_ФИО";

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value;
        }
    }
}
