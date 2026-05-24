using Microsoft.Win32;
using PsyDiagnostics.Models;
using PsyDiagnostics.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            var chartBytes = BuildPersonalAnalyticsChart(tests);

            var dynamicsGroups = BuildDynamicsGroups(tests);

            // Диаграмма строится одна общая по всем 8 шкалам.
            // При переносе блока аналитики диаграмма повторяется слева, но заголовки не дублируются.
            var dynamicsChunks = dynamicsGroups
                .Select((group, index) => new { group, index })
                .GroupBy(x => x.index / 4)
                .Select(g => g.Select(x => x.group).ToList())
                .ToList();

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

                        if (chartBytes != null)
                        {
                            col.Item().PaddingTop(10).Text("Персональная аналитика")
                                .FontSize(16)
                                .Bold();

                            col.Item().Text(
                                "Диаграмма отражает динамику результатов обследуемого по датам прохождения тестирования. " +
                                "Улучшение рассчитывается путем сравнения первого и последнего результата по каждой методике."
                            );

                            if (dynamicsChunks.Count == 0)
                            {
                                col.Item().Text("Для расчета динамики необходимо не менее двух результатов по одной методике.");
                            }
                            else
                            {
                                for (int i = 0; i < dynamicsChunks.Count; i++)
                                {
                                    var chunk = dynamicsChunks[i];
                                    bool isFirstBlock = i == 0;

                                    col.Item().PaddingTop(isFirstBlock ? 10 : 4).Row(row =>
                                    {
                                        row.RelativeItem(1.2f).Column(left =>
                                        {
                                            if (isFirstBlock)
                                            {
                                                left.Item().Text("Динамика результатов по шкалам")
                                                    .FontSize(14)
                                                    .Bold();
                                            }

                                            left.Item()
                                                .PaddingTop(isFirstBlock ? 8 : 0)
                                                .Image(chartBytes)
                                                .FitWidth();
                                        });

                                        row.RelativeItem(1).PaddingLeft(20).Column(right =>
                                        {
                                            if (isFirstBlock)
                                            {
                                                right.Item().Text("Динамика по шкалам")
                                                    .FontSize(14)
                                                    .Bold();
                                            }

                                            foreach (var group in chunk)
                                            {
                                                AddDynamicsBlock(right, group.Key, group.ToList());
                                            }
                                        });
                                    });
                                }
                            }
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

        private static List<IGrouping<string, DynamicsPoint>> BuildDynamicsGroups(List<TestHistoryItem> tests)
        {
            return tests
                .Select(x => new DynamicsPoint
                {
                    TestName = x.TestName,
                    Score = x.Score,
                    Date = ParseDate(x.Date)
                })
                .Where(x => x.Date.HasValue &&
                            !string.IsNullOrWhiteSpace(x.TestName))
                .GroupBy(x => x.TestName)
                .Where(g => g.Count() >= 2)
                .OrderBy(g => GetScaleOrder(g.Key))
                .ToList();
        }

        private static void AddDynamicsBlock(
            ColumnDescriptor right,
            string testName,
            List<DynamicsPoint> points)
        {
            var ordered = points
                .Where(x => x.Date.HasValue)
                .OrderBy(x => x.Date.Value)
                .ToList();

            if (ordered.Count < 2)
                return;

            var first = ordered.First();
            var last = ordered.Last();

            int diff = last.Score - first.Score;

            bool isPositive =
                ScaleDirections.TryGetValue(testName, out bool val)
                && val;

            string result;

            if (diff == 0)
                result = "Без выраженной динамики";
            else if (isPositive)
                result = diff > 0 ? "Улучшение" : "Ухудшение";
            else
                result = diff < 0 ? "Улучшение" : "Ухудшение";

            string resultColor =
                result == "Улучшение"
                    ? Colors.Green.Medium
                    : result == "Ухудшение"
                        ? Colors.Red.Medium
                        : Colors.Grey.Medium;

            right.Item()
                .PaddingBottom(10)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingBottom(6)
                .Column(item =>
                {
                    item.Item().Text(testName)
                        .Bold()
                        .FontSize(11);

                    item.Item().Text(
                        $"Первый результат: {first.Date.Value:dd.MM.yyyy} — {first.Score}"
                    ).FontSize(9);

                    item.Item().Text(
                        $"Последний результат: {last.Date.Value:dd.MM.yyyy} — {last.Score}"
                    ).FontSize(9);

                    item.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9));

                        text.Span("Вывод: ");
                        text.Span(result)
                            .FontColor(resultColor)
                            .Bold();
                    });
                });
        }

        private static byte[] BuildPersonalAnalyticsChart(List<TestHistoryItem> tests)
        {
            var points = tests
                .Select(x => new DynamicsPoint
                {
                    TestName = x.TestName,
                    Score = x.Score,
                    Date = ParseDate(x.Date)
                })
                .Where(x => x.Date.HasValue &&
                            !string.IsNullOrWhiteSpace(x.TestName))
                .GroupBy(x => x.TestName)
                .Where(g => g.Any())
                .OrderBy(g => GetScaleOrder(g.Key))
                .ToList();

            if (points.Count == 0)
                return null;

            const int width = 1000;
            const int height = 560;

            const int left = 80;
            const int right = 40;
            const int top = 55;
            const int bottom = 130;

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColors.White);

            using var axisPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                IsAntialias = true
            };

            using var gridPaint = new SKPaint
            {
                Color = new SKColor(220, 220, 220),
                StrokeWidth = 1,
                IsAntialias = true
            };

            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 22,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            using var smallTextPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            canvas.DrawText("Динамика персональной аналитики", left, 32, textPaint);

            float chartWidth = width - left - right;
            float chartHeight = height - top - bottom;

            canvas.DrawLine(left, top, left, top + chartHeight, axisPaint);
            canvas.DrawLine(left, top + chartHeight, left + chartWidth, top + chartHeight, axisPaint);

            for (int i = 0; i <= 5; i++)
            {
                float y = top + chartHeight - chartHeight * i / 5;
                int value = i * 20;

                canvas.DrawLine(left, y, left + chartWidth, y, gridPaint);
                canvas.DrawText(value.ToString(), 35, y + 6, smallTextPaint);
            }

            var allDates = points
                .SelectMany(g => g.Select(x => x.Date.Value.Date))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (allDates.Count == 0)
                return null;

            var colors = new[]
            {
                SKColors.RoyalBlue,
                SKColors.Firebrick,
                SKColors.ForestGreen,
                SKColors.DarkOrange,
                SKColors.Purple,
                SKColors.Teal,
                SKColors.Brown,
                SKColors.DeepPink
            };

            float GetX(DateTime date)
            {
                if (allDates.Count == 1)
                    return left + chartWidth / 2;

                int index = allDates.IndexOf(date.Date);
                return left + chartWidth * index / (allDates.Count - 1);
            }

            float GetY(int score)
            {
                score = Math.Max(0, Math.Min(100, score));
                return top + chartHeight - chartHeight * score / 100f;
            }

            for (int i = 0; i < allDates.Count; i++)
            {
                float x = GetX(allDates[i]);

                canvas.DrawLine(
                    x,
                    top + chartHeight,
                    x,
                    top + chartHeight + 5,
                    axisPaint);

                canvas.DrawText(
                    allDates[i].ToString("dd.MM.yyyy"),
                    x - 45,
                    top + chartHeight + 30,
                    smallTextPaint);
            }

            int colorIndex = 0;

            foreach (var group in points)
            {
                var ordered = group
                    .OrderBy(x => x.Date.Value)
                    .ToList();

                var color = colors[colorIndex % colors.Length];

                using var linePaint = new SKPaint
                {
                    Color = color,
                    StrokeWidth = 4,
                    IsAntialias = true
                };

                using var dotPaint = new SKPaint
                {
                    Color = color,
                    IsAntialias = true
                };

                for (int i = 0; i < ordered.Count; i++)
                {
                    float x = GetX(ordered[i].Date.Value);
                    float y = GetY(ordered[i].Score);

                    canvas.DrawCircle(x, y, 6, dotPaint);

                    if (i > 0)
                    {
                        float prevX = GetX(ordered[i - 1].Date.Value);
                        float prevY = GetY(ordered[i - 1].Score);

                        canvas.DrawLine(prevX, prevY, x, y, linePaint);
                    }
                }

                float legendX = left + (colorIndex % 2) * 420;
                float legendY = height - 48 + (colorIndex / 2) * 20;

                canvas.DrawCircle(legendX, legendY - 5, 6, dotPaint);
                canvas.DrawText(group.Key, legendX + 15, legendY, smallTextPaint);

                colorIndex++;
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return data.ToArray();
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] formats =
            {
                "dd.MM.yyyy",
                "dd.MM.yyyy HH:mm",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss"
            };

            if (DateTime.TryParseExact(
                    value,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var exactDate))
                return exactDate;

            if (DateTime.TryParse(value, out var parsedDate))
                return parsedDate;

            return null;
        }

        private static readonly Dictionary<string, bool> ScaleDirections = new()
        {
            // false = меньше -> лучше
            // true = больше -> лучше

            { "Уровень агрессивности", false },
            { "Импульсивность", false },
            { "Депрессивное состояние", false },
            { "Тревожность", false },
            { "Враждебность", false },

            { "Стрессоустойчивость", true },
            { "Социальная адаптация", true },
            { "Психологическая устойчивость", true }
        };

        private static int GetScaleOrder(string name)
        {
            return name switch
            {
                "Уровень агрессивности" => 1,
                "Импульсивность" => 2,
                "Депрессивное состояние" => 3,
                "Стрессоустойчивость" => 4,
                "Социальная адаптация" => 5,
                "Тревожность" => 6,
                "Психологическая устойчивость" => 7,
                "Враждебность" => 8,
                _ => 100
            };
        }

        private sealed class DynamicsPoint
        {
            public string TestName { get; set; }
            public int Score { get; set; }
            public DateTime? Date { get; set; }
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

            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value;
        }
    }
}
