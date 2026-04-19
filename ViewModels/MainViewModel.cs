using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using PsyDiagnostics.Views;
using PsyDiagnostics.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using QuestPDF.Infrastructure;
using LiveChartsCore.Kernel.Sketches;

namespace PsyDiagnostics.ViewModels
{
    public class TestHistoryItem : BaseViewModel
    {
        public string TestName { get; set; }
        public int Score { get; set; }
        public string Risk { get; set; }
        public string Date { get; set; }

        // добавил для фильтрации по отряду
        public string FullName { get; set; }
        public string Unit { get; set; }
    }

    public enum AnalyticsSection
    {
        [Description("Персональная аналитика")]
        ПерсональнаяАналитика,

        [Description("Аналитика по отрядам")]
        АналитикаПоОтрядам,

        [Description("Общая аналитика")]
        ОбщаяАналитика
    }

    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ParticipantViewModel ParticipantVm { get; }

        private string _searchId;
        public string SearchId
        {
            get => _searchId;
            set { _searchId = value; OnPropertyChanged(); }
        }

        private Participant _current;
        public Participant Current
        {
            get => _current;
            set
            {
                if (_current != null)
                    _current.PropertyChanged -= Current_PropertyChanged;

                _current = value;

                if (_current != null)
                    _current.PropertyChanged += Current_PropertyChanged;

                ParticipantVm.CurrentParticipant = _current;

                SelectedArticle = AllArticles
                    .FirstOrDefault(a => a.Number?.Trim() == _current?.ArticleNumber?.Trim());

                OnPropertyChanged(nameof(Current));
                OnPropertyChanged(nameof(SelectedArticle));
                OnPropertyChanged(nameof(CanSave));

                UpdateUnitRisk();
                LoadTestHistory();
                BuildPersonalChart();
                BuildPersonalAiSummary();
            }
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Current));
            OnPropertyChanged(nameof(CanSave));
        }

        public Array EducationLevels => Enum.GetValues(typeof(EducationLevel));
        public Array MaritalStatuses => Enum.GetValues(typeof(MaritalStatus));
        public Array CrimeTypes => Enum.GetValues(typeof(CrimeType));
        public Array Recidivisms => Enum.GetValues(typeof(Recidivism));
        public Array Categories => Enum.GetValues(typeof(Category));
        public Array Citizenships => Enum.GetValues(typeof(Citizenship));

        public ICommand SearchCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoToTestCommand { get; }
        public ICommand CalculateRiskCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand ExportPdfCommand { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public List<Article> AllArticles { get; set; } = new List<Article>();

        private List<Article> _filteredArticles;
        public List<Article> FilteredArticles
        {
            get => _filteredArticles;
            set { _filteredArticles = value; OnPropertyChanged(); }
        }

        private string _articleSearch;
        public string ArticleSearch
        {
            get => _articleSearch;
            set
            {
                _articleSearch = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(value))
                {
                    FilteredArticles = AllArticles;
                }
                else
                {
                    var lower = value.ToLower();

                    FilteredArticles = AllArticles
                        .Where(a => a.Number.Contains(value)
                                 || a.Title.ToLower().Contains(lower))
                        .ToList();
                }
            }
        }

        private Article _selectedArticle;
        public Article SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                _selectedArticle = value;
                OnPropertyChanged();

                if (value == null)
                    return;

                if (Current != null)
                {
                    Current.ArticleNumber = value.Number;
                    Current.ArticlePart = value.Parts?.FirstOrDefault();
                    Current.ArticlePoint = value.Points?.FirstOrDefault();
                }

                OnPropertyChanged(nameof(AvailableParts));
                OnPropertyChanged(nameof(AvailablePoints));
            }
        }

        public List<string> AvailableParts => SelectedArticle?.Parts ?? new List<string>();
        public List<string> AvailablePoints => SelectedArticle?.Points ?? new List<string>();

        private ObservableCollection<TestHistoryItem> _testHistory =
            new ObservableCollection<TestHistoryItem>();

        public ObservableCollection<TestHistoryItem> TestHistory
        {
            get => _testHistory;
            set { _testHistory = value; OnPropertyChanged(); }
        }
        public ObservableCollection<TestHistoryItem> AggressionHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> ImpulsivityHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> DepressionHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> StressHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> AdaptationHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> AnxietyHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> ResilienceHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> HostilityHistory { get; set; } = new();

        private bool _canGoHomeAfterTests;
        public bool CanGoHomeAfterTests
        {
            get => _canGoHomeAfterTests;
            set { _canGoHomeAfterTests = value; OnPropertyChanged(); }
        }

        public TestMode SelectedMode { get; set; }

        public MainViewModel()
        {
            ParticipantVm = new ParticipantViewModel();

            SearchCommand = new RelayCommand(_ => Search());
            SaveCommand = new RelayCommand(_ => Save());
            GoToTestCommand = new RelayCommand(_ => GoToTest());
            CalculateRiskCommand = new RelayCommand(_ => CalculateRisk());
            GoHomeCommand = new RelayCommand(_ => GoHome());
            ExportPdfCommand = new RelayCommand(_ => ExportPdf());

            AllArticles = JsonHelper.LoadArticles();
            FilteredArticles = AllArticles;


            Units = _db.GetUnits();
            OnPropertyChanged(nameof(Units));

            if (Units.Any())
                SelectedUnit = Units.First();



            ShowParticipant();
            BuildRiskByUnitsChart();
            BuildRecidivismChart();
            BuildTopUnitsChart();
        }
        public SolidColorPaint PersonalLegendTextPaint { get; set; } = new SolidColorPaint(SKColors.White);
        public Array AnalyticsSections => Enum.GetValues(typeof(AnalyticsSection));

        private AnalyticsSection _selectedAnalyticsSection = AnalyticsSection.ПерсональнаяАналитика;
        public AnalyticsSection SelectedAnalyticsSection
        {
            get => _selectedAnalyticsSection;
            set
            {
                _selectedAnalyticsSection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGeneralAnalyticsVisible));
                OnPropertyChanged(nameof(IsUnitAnalyticsVisible));
                OnPropertyChanged(nameof(IsPersonalAnalyticsVisible));
            }
        }

        public Visibility IsGeneralAnalyticsVisible =>
            SelectedAnalyticsSection == AnalyticsSection.ОбщаяАналитика
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility IsUnitAnalyticsVisible =>
            SelectedAnalyticsSection == AnalyticsSection.АналитикаПоОтрядам
                ? Visibility.Visible
                : Visibility.Collapsed;

        public Visibility IsPersonalAnalyticsVisible =>
            SelectedAnalyticsSection == AnalyticsSection.ПерсональнаяАналитика
                ? Visibility.Visible
                : Visibility.Collapsed;

        private void ShowParticipant()
        {
            ParticipantVm.OnNavigateToTest = participant =>
            {
                Current = participant;
                GoToTest();
            };

            CurrentView = new ParticipantView { DataContext = this };
        }

        private void GoToTest()
        {
            if (Current == null)
            {
                MessageBox.Show("Сначала найдите участника");
                return;
            }

            CanGoHomeAfterTests = false;

            var modeVm = new ModeSelectionViewModel(this);

            modeVm.OnBack = () =>
            {
                ShowParticipant();
            };

            modeVm.OnModeSelected = mode =>
            {
                SelectedMode = mode;

                var defs = TestLoader.LoadAll();

                if (defs == null || defs.Count == 0)
                {
                    MessageBox.Show("Не удалось загрузить тесты.");
                    return;
                }

                if (mode == TestMode.Full)
                {
                    var multiVm = new MultiTestViewModel(this, defs, mode);
                    CurrentView = new MultiTestView { DataContext = multiVm };
                    return;
                }

                var selectVm = new TestSelectionViewModel(defs, mode);

                selectVm.OnBack = () =>
                {
                    ShowParticipant();
                };

                switch (mode)
                {
                    case TestMode.Express:
                        selectVm.OnStart = (selectedDefs, m) =>
                        {
                            var def = selectedDefs.First();
                            var testVm = new TestViewModel(this, def, m);

                            testVm.OnFinished += () =>
                            {
                                ShowResult(testVm.GetResults());
                            };

                            CurrentView = new TestView { DataContext = testVm };
                        };
                        break;

                    case TestMode.Normal:
                        selectVm.OnStart = (selectedDefs, m) =>
                        {
                            var multiVm = new MultiTestViewModel(this, selectedDefs, m);
                            CurrentView = new MultiTestView { DataContext = multiVm };
                        };
                        break;

                    case TestMode.Full:
                        break;
                }

                CurrentView = new TestSelectionView { DataContext = selectVm };
            };

            CurrentView = new ModeSelectionView { DataContext = modeVm };
        }

        private void ShowTesting()
        {
            var vm = new TestingViewModel(this);
            CurrentView = new TestingView { DataContext = vm };
        }

        public void ShowResult(Dictionary<string, int> results)
        {
            if (Current == null)
            {
                MessageBox.Show("Нет участника");
                return;
            }

            var report = _db.GetFullReport(Current.PrisonerId);

            if (report.aiResults == null || report.aiResults.Count == 0)
            {
                MessageBox.Show("Нет результатов");
                return;
            }

            string text = "РЕЗУЛЬТАТЫ:\n\n";

            foreach (var r in report.aiResults)
            {
                string risk = r.Prediction == 1
                    ? "Высокий риск"
                    : "Низкий риск";

                text += $"{r.TestName}\n" +
                        $"Баллы: {r.Score}\n" +
                        $"Прогноз: {risk}\n" +
                        $"Дата: {r.Date}\n\n";
            }

            MessageBox.Show(text);

            LoadTestHistory();

            // после теста тоже обновим график/список
            UpdateChart();

            CanGoHomeAfterTests = true;
        }

        private void LoadTestHistory()
        {
            TestHistory.Clear();

            AggressionHistory.Clear();
            ImpulsivityHistory.Clear();
            DepressionHistory.Clear();
            StressHistory.Clear();
            AdaptationHistory.Clear();
            AnxietyHistory.Clear();
            ResilienceHistory.Clear();
            HostilityHistory.Clear();

            if (Current == null)
                return;

            var report = _db.GetFullReport(Current.PrisonerId);

            if (report.aiResults == null)
                return;

            foreach (var r in report.aiResults.OrderByDescending(x => x.Date))
            {
                string risk = r.Prediction == 1 ? "Высокий риск" : "Низкий риск";

                var item = new TestHistoryItem
                {
                    TestName = r.TestName,
                    Score = r.Score,
                    Risk = risk,
                    Date = r.Date,
                    FullName = Current.FullName,
                    Unit = Current.Unit
                };

                TestHistory.Add(item);

                switch (r.TestName)
                {
                    case "Aggression":
                        AggressionHistory.Add(item);
                        break;
                    case "Impulsivity":
                        ImpulsivityHistory.Add(item);
                        break;
                    case "Depression":
                        DepressionHistory.Add(item);
                        break;
                    case "Stress":
                        StressHistory.Add(item);
                        break;
                    case "Adaptation":
                        AdaptationHistory.Add(item);
                        break;
                    case "Anxiety":
                        AnxietyHistory.Add(item);
                        break;
                    case "Resilience":
                        ResilienceHistory.Add(item);
                        break;
                    case "Hostility":
                        HostilityHistory.Add(item);
                        break;
                }
            }

            OnPropertyChanged(nameof(AggressionHistory));
            OnPropertyChanged(nameof(ImpulsivityHistory));
            OnPropertyChanged(nameof(DepressionHistory));
            OnPropertyChanged(nameof(StressHistory));
            OnPropertyChanged(nameof(AdaptationHistory));
            OnPropertyChanged(nameof(AnxietyHistory));
            OnPropertyChanged(nameof(ResilienceHistory));
            OnPropertyChanged(nameof(HostilityHistory));
        }

        private void ExportPdf()
        {
            if (Current == null)
            {
                MessageBox.Show("Нет участника");
                return;
            }

            MessageBox.Show("Выгрузка PDF пока не реализована.");
        }

        private string _unitRisk;
        public string UnitRisk
        {
            get => _unitRisk;
            set
            {
                _unitRisk = value;
                OnPropertyChanged();
            }
        }

        private string _unitStats;
        public string UnitStats
        {
            get => _unitStats;
            set
            {
                _unitStats = value;
                OnPropertyChanged();
            }
        }
        public void UpdateUnitRisk()
        {
            if (Current == null || string.IsNullOrEmpty(Current.Unit))
            {
                UnitRisk = "Нет данных";
                UnitStats = "";
                return;
            }

            double avg = _db.GetAverageRiskByUnit(Current.Unit);

            string level;
            if (avg > 66)
            {
                level = "Высокий";
                UnitRiskColor = "#FF5252";
            }
            else if (avg > 32)
            {
                level = "Средний";
                UnitRiskColor = "#FFC107";
            }
            else
            {
                level = "Низкий";
                UnitRiskColor = "#4CAF50";
            }

            UnitRisk = $"{level} ({avg:F0}%)";

            var stats = _db.GetUnitStats(Current.Unit);

            UnitCount = stats.count;
            int low = (int)stats.low;
            int mid = (int)stats.mid;
            int high = (int)stats.high;

            UnitStats = $"Отряд: {Current.Unit} | Человек: {UnitCount}";

            var series = new List<ISeries>();

            if (low > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { low },
                    Name = "Низкий"
                });
            }

            if (mid > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { mid },
                    Name = "Средний"
                });
            }

            if (high > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { high },
                    Name = "Высокий"
                });
            }

            RiskDistributionSeries = series.ToArray();

            TopPeople.Clear();

            var top = _db.GetTopPeopleFromBestUnit();

            foreach (var p in top)
            {
                TopPeople.Add($"{p.name} {p.unit} отряд - {(int)p.risk} баллов");
            }

            LoadRiskPeople();

            BuildRiskByUnitsChart();
            BuildRecidivismChart();
            BuildTopUnitsChart();
            BuildPersonalChart();

            OnPropertyChanged(nameof(TopPeople));
            OnPropertyChanged(nameof(RiskPeople));
            OnPropertyChanged(nameof(RiskDistributionSeries));
        }

        private string _personalAiConclusion;
        public string PersonalAiConclusion
        {
            get => _personalAiConclusion;
            set
            {
                _personalAiConclusion = value;
                OnPropertyChanged();
            }
        }

        private string _personalAiRisk;
        public string PersonalAiRisk
        {
            get => _personalAiRisk;
            set
            {
                _personalAiRisk = value;
                OnPropertyChanged();
            }
        }

        private string _personalAiRecommendations;
        public string PersonalAiRecommendations
        {
            get => _personalAiRecommendations;
            set
            {
                _personalAiRecommendations = value;
                OnPropertyChanged();
            }
        }

        private void BuildPersonalAiSummary()
        {
            if (Current == null)
            {
                PersonalAiConclusion = "Нет данных для анализа.";
                PersonalAiRisk = "";
                PersonalAiRecommendations = "";
                return;
            }

            var report = _db.GetFullReport(Current.PrisonerId);

            if (report.aiResults == null || report.aiResults.Count == 0)
            {
                PersonalAiConclusion = "Нет результатов тестирования.";
                PersonalAiRisk = "";
                PersonalAiRecommendations = "";
                return;
            }

            var relevantTests = new[]
            {
        "Aggression",
        "Impulsivity",
        "Depression",
        "Stress",
        "Adaptation",
        "Anxiety",
        "Resilience",
        "Hostility"
    };

            var grouped = report.aiResults
                .Where(x => relevantTests.Contains(x.TestName) && !string.IsNullOrWhiteSpace(x.Date))
                .GroupBy(x => x.TestName)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => DateTime.Parse(x.Date)).ToList()
                );

            int improved = 0;
            int worsened = 0;
            var improvedTests = new List<string>();
            var worsenedTests = new List<string>();

            foreach (var pair in grouped)
            {
                var testName = pair.Key;
                var items = pair.Value;

                if (items.Count < 2)
                    continue;

                var first = items.First().Score;
                var last = items.Last().Score;

                bool higherIsBetter = testName == "Stress" || testName == "Adaptation" || testName == "Resilience";

                bool isImproved = higherIsBetter ? last > first : last < first;
                bool isWorsened = higherIsBetter ? last < first : last > first;

                if (isImproved)
                {
                    improved++;
                    improvedTests.Add(GetTestDisplayName(testName));
                }
                else if (isWorsened)
                {
                    worsened++;
                    worsenedTests.Add(GetTestDisplayName(testName));
                }
            }

            var allResultsOrdered = report.aiResults
                .Where(x => !string.IsNullOrWhiteSpace(x.Date))
                .OrderBy(x => DateTime.Parse(x.Date))
                .ToList();

            var startPeriod = DateTime.Parse(allResultsOrdered.First().Date).ToString("dd.MM.yyyy");
            var endPeriod = DateTime.Parse(allResultsOrdered.Last().Date).ToString("dd.MM.yyyy");

            var latestRisk = allResultsOrdered
                .OrderByDescending(x => DateTime.Parse(x.Date))
                .FirstOrDefault()?.RiskScore ?? 0;

            string riskLevel;
            if (latestRisk <= 32)
                riskLevel = "Низкий риск";
            else if (latestRisk <= 66)
                riskLevel = "Средний риск";
            else
                riskLevel = "Высокий риск";

            string improveText = improvedTests.Count > 0
                ? $"Улучшения отмечены по шкалам: {string.Join(", ", improvedTests)}."
                : "Выраженных улучшений по ключевым шкалам не выявлено.";

            string worsenText = worsenedTests.Count > 0
                ? $"Негативная динамика отмечена по шкалам: {string.Join(", ", worsenedTests)}."
                : "Негативной динамики по ключевым шкалам не выявлено.";

            PersonalAiConclusion =
                $"За период с {startPeriod} по {endPeriod} показатели обследуемого были проанализированы по 8 психологическим шкалам. " +
                $"{improveText} {worsenText} По совокупности последних результатов наблюдается: {riskLevel.ToLower()}.";

            PersonalAiRisk = $"Итоговый прогноз нейросети: {riskLevel}.";

            if (riskLevel == "Низкий риск")
            {
                PersonalAiRecommendations =
                    "Рекомендации: продолжить наблюдение в плановом порядке, поддерживать положительную динамику, " +
                    "закреплять адаптационные навыки, вовлекать в конструктивные виды деятельности.";
            }
            else if (riskLevel == "Средний риск")
            {
                PersonalAiRecommendations =
                    "Рекомендации: усилить индивидуальную профилактическую работу, контролировать эмоциональное состояние, " +
                    "обратить внимание на проблемные шкалы и провести повторную диагностику в динамике.";
            }
            else
            {
                PersonalAiRecommendations =
                    "Рекомендации: требуется повышенное внимание психолога и сотрудников, индивидуальная коррекционная работа, " +
                    "мониторинг факторов дезадаптации, агрессии, тревожности и иных проблемных показателей.";
            }
        }
        public List<string> Units { get; set; }

        private string _selectedUnit;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged();

                    LoadRiskPeople();
                    UpdateChart();
                }
            }
        }

        private void LoadRiskPeople()
        {
            RiskPeople.Clear();
            LowRiskPeople.Clear();
            MediumRiskPeople.Clear();
            HighRiskPeople.Clear();

            if (string.IsNullOrWhiteSpace(SelectedUnit))
                return;

            var data = _db.GetAllPeopleWithRisk(SelectedUnit);

            foreach (var p in data)
            {
                string text = $"{p.name} — {(int)p.risk} баллов";

                RiskPeople.Add($"{p.name} {p.unit} отряд - {(int)p.risk} баллов");

                if (p.risk >= 0 && p.risk <= 32)
                    LowRiskPeople.Add(text);
                else if (p.risk >= 33 && p.risk <= 66)
                    MediumRiskPeople.Add(text);
                else
                    HighRiskPeople.Add(text);
            }

            OnPropertyChanged(nameof(RiskPeople));
            OnPropertyChanged(nameof(LowRiskPeople));
            OnPropertyChanged(nameof(MediumRiskPeople));
            OnPropertyChanged(nameof(HighRiskPeople));
        }

        private void LoadFilteredData()
        {
            FilteredHistory.Clear();

            var filtered = AllHistory
                .Where(x => x.Unit == SelectedUnit)
                .ToList();

            foreach (var item in filtered)
                FilteredHistory.Add(item);

            OnPropertyChanged(nameof(FilteredHistory));
        }

        // ГЛАВНОЕ: диаграмма по выбранному отряду
        private void UpdateChart()
        {
            if (string.IsNullOrWhiteSpace(SelectedUnit))
                return;

            var stats = _db.GetUnitStats(SelectedUnit);

            UnitCount = stats.count;
            int low = (int)stats.low;
            int mid = (int)stats.mid;
            int high = (int)stats.high;

            UnitStats = $"Отряд: {SelectedUnit} | Человек: {UnitCount}";

            double avg = _db.GetAverageRiskByUnit(SelectedUnit);

            string level;
            if (avg > 66)
            {
                level = "Высокий";
                UnitRiskColor = "#FF5252";
            }
            else if (avg > 32)
            {
                level = "Средний";
                UnitRiskColor = "#FFC107";
            }
            else
            {
                level = "Низкий";
                UnitRiskColor = "#4CAF50";
            }

            UnitRisk = $"{level} ({avg:F0}%)";

            var series = new List<ISeries>();

            if (low > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { low },
                    Name = "Низкий"
                });
            }

            if (mid > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { mid },
                    Name = "Средний"
                });
            }

            if (high > 0)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new List<double> { high },
                    Name = "Высокий"
                });
            }

            RiskDistributionSeries = series.ToArray();

            OnPropertyChanged(nameof(UnitStats));
            OnPropertyChanged(nameof(UnitRisk));
            OnPropertyChanged(nameof(UnitRiskColor));
            OnPropertyChanged(nameof(RiskDistributionSeries));
        }

        public void BuildRiskByUnitsChart()
        {
            var data = _db.GetRiskByUnits();

            RiskByUnitSeries = new ISeries[]
            {
        new ColumnSeries<double>
        {
            Values = data.Select(x => x.avgRisk).ToList(),
            DataLabelsFormatter = p => $"{p.Model:F0}%"
        }
            };

            UnitXAxis = new Axis[]
            {
        new Axis
        {
            Labels = data.Select(x => $"Отряд {x.unit}").ToArray()
        }
            };

            RiskYAxis = new Axis[]
            {
        new Axis
        {
            MinLimit = 0
        }
            };

            OnPropertyChanged(nameof(RiskByUnitSeries));
            OnPropertyChanged(nameof(UnitXAxis));
            OnPropertyChanged(nameof(RiskYAxis));
        }
        public void BuildRecidivismChart()
        {
            var data = _db.GetRecidivismStats();

            double first = data.first;
            double repeat = data.repeat;
            double total = first + repeat;

            RecidivismSeries = new ISeries[]
            {
        new ColumnSeries<double>
        {
            Values = new double[] { first, repeat },
            DataLabelsFormatter = point =>
            {
                var value = point.Coordinate.PrimaryValue;
                double percent = total == 0 ? 0 : (value / total) * 100;
                return $"{value:F0}%";
            }
        }
            };

            RecidivismXAxis = new Axis[]
            {
        new Axis
        {
            Labels = new[]
            {
                "Первоходы",
                "Второходы"
            }
        }
            };

            OnPropertyChanged(nameof(RecidivismSeries));
            OnPropertyChanged(nameof(RecidivismXAxis));
        }

        public void BuildTopUnitsChart()
        {
            var data = _db.GetTopUnitsImprovement();

            if (data == null || data.Count == 0)
                return;

            double total = data.Sum(x => x.improvement);

            TopUnitsSeries = new ISeries[]
            {
        new ColumnSeries<double>
        {
            Values = data.Select(x => (double)x.improvement).ToArray(),
            DataLabelsFormatter = point =>
            {
                double value = point.Coordinate.PrimaryValue;
                return $"{value:F0}";
            }
        }
            };

            TopUnitsXAxis = new Axis[]
            {
        new Axis
        {
            Labels = data.Select(x => $"Отряд {x.unit}").ToArray()
        }
            };

            OnPropertyChanged(nameof(TopUnitsSeries));
            OnPropertyChanged(nameof(TopUnitsXAxis));
        }
        public void BuildPersonalChart()
        {
            if (Current == null)
            {
                PersonalRiskSeries = Array.Empty<ISeries>();
                DateXAxis = Array.Empty<Axis>();
                PersonalYAxis = Array.Empty<Axis>();

                OnPropertyChanged(nameof(PersonalRiskSeries));
                OnPropertyChanged(nameof(DateXAxis));
                OnPropertyChanged(nameof(PersonalYAxis));
                return;
            }

            var report = _db.GetFullReport(Current.PrisonerId);

            if (report.aiResults == null || report.aiResults.Count == 0)
            {
                PersonalRiskSeries = Array.Empty<ISeries>();
                DateXAxis = Array.Empty<Axis>();
                PersonalYAxis = Array.Empty<Axis>();

                OnPropertyChanged(nameof(PersonalRiskSeries));
                OnPropertyChanged(nameof(DateXAxis));
                OnPropertyChanged(nameof(PersonalYAxis));
                return;
            }

            var ordered = report.aiResults
                .Where(x => !string.IsNullOrWhiteSpace(x.TestName) && !string.IsNullOrWhiteSpace(x.Date))
                .OrderBy(x => DateTime.Parse(x.Date))
                .ToList();

            var dates = ordered
                .Select(x => DateTime.Parse(x.Date).ToString("dd.MM"))
                .Distinct()
                .ToArray();

            var testNames = new[]
            {
        "Aggression",
        "Impulsivity",
        "Depression",
        "Stress",
        "Adaptation",
        "Anxiety",
        "Resilience",
        "Hostility"
    };

            var seriesList = new List<ISeries>();

            foreach (var testName in testNames)
            {
                var values = new List<double?>();

                foreach (var date in dates)
                {
                    var item = ordered
                        .Where(x => x.TestName == testName &&
                                    DateTime.Parse(x.Date).ToString("dd.MM") == date)
                        .OrderByDescending(x => DateTime.Parse(x.Date))
                        .FirstOrDefault();

                    values.Add(item != null ? item.Score : null);
                }

                if (values.Any(v => v.HasValue))
                {
                    seriesList.Add(new LineSeries<double?>
                    {
                        Name = GetTestDisplayName(testName),
                        Values = values.ToArray(),
                        GeometrySize = 5,
                        LineSmoothness = 0,
                        Fill = null
                    });
                }
            }

            PersonalRiskSeries = seriesList.ToArray();

            DateXAxis = new Axis[]
 {
    new Axis
    {
        Labels = dates,
        MinStep = 1,
        ForceStepToMin = true,
        TextSize = 11,
        LabelsRotation = 0,
        LabelsPaint = new SolidColorPaint(new SKColor(245, 245, 247)),
        SeparatorsPaint = new SolidColorPaint(new SKColor(120, 120, 140))
    }
 };

            PersonalYAxis = new Axis[]
            {
    new Axis
    {
        MinLimit = 0,
        MaxLimit = 100,
        MinStep = 10,
        ForceStepToMin = true,
        TextSize = 11,
        LabelsPaint = new SolidColorPaint(new SKColor(245, 245, 247)),
        SeparatorsPaint = new SolidColorPaint(new SKColor(120, 120, 140))
    }
            };

            OnPropertyChanged(nameof(PersonalRiskSeries));
            OnPropertyChanged(nameof(DateXAxis));
            OnPropertyChanged(nameof(PersonalYAxis));
        }
        private string GetTestDisplayName(string testName)
        {
            return testName switch
            {
                "Aggression" => "Агрессивность",
                "Impulsivity" => "Импульсивность",
                "Depression" => "Депрессия",
                "Stress" => "Стрессоустойчивость",
                "Adaptation" => "Адаптация",
                "Anxiety" => "Тревожность",
                "Resilience" => "Устойчивость",
                "Hostility" => "Враждебность",
                _ => testName
            };
        }
        public Axis[] PersonalYAxis { get; set; }
        public ObservableCollection<string> TopPeople { get; set; } = new();
        public ObservableCollection<string> RiskPeople { get; set; } = new();
        public ObservableCollection<string> LowRiskPeople { get; set; } = new();
        public ObservableCollection<string> MediumRiskPeople { get; set; } = new();
        public ObservableCollection<string> HighRiskPeople { get; set; } = new();
        public ObservableCollection<TestHistoryItem> AllHistory { get; set; } = new();
        public ObservableCollection<TestHistoryItem> FilteredHistory { get; set; } = new();

        private string _unitRiskColor;
        public string UnitRiskColor
        {
            get => _unitRiskColor;
            set
            {
                _unitRiskColor = value;
                OnPropertyChanged();
            }
        }

        private int _unitCount;
        public int UnitCount
        {
            get => _unitCount;
            set
            {
                _unitCount = value;
                OnPropertyChanged();
            }
        }

        private int _highRiskPercent;
        public int HighRiskPercent
        {
            get => _highRiskPercent;
            set
            {
                _highRiskPercent = value;
                OnPropertyChanged();
            }
        }

        private void Search()
        {
            try
            {
                var id = SearchId?.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    MessageBox.Show("Введите ID");
                    return;
                }

                var found = _db.GetParticipant(id);

                if (found == null)
                {
                    Current = new Participant
                    {
                        PrisonerId = id,
                        BirthDate = DateTime.Today
                    };

                    MessageBox.Show("Не найден");
                    return;
                }

                Current = found;

                FilteredArticles = AllArticles;

                var article = AllArticles
                    .FirstOrDefault(a =>
                        a.Number?.Trim() == Current.ArticleNumber?.Trim());

                SelectedArticle = article;

                OnPropertyChanged(nameof(SelectedArticle));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message);
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(SearchId))
            {
                MessageBox.Show("Введите ID");
                return;
            }

            if (Current == null)
            {
                MessageBox.Show("Сначала нажмите Найти");
                return;
            }

            var errors = Current.GetErrors();
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибки");
                return;
            }

            _db.SaveParticipant(Current);
            MessageBox.Show("Сохранено");

            // после сохранения обновим визуализацию выбранного отряда
            LoadRiskPeople();
            UpdateChart();
        }

        private void CalculateRisk()
        {
            if (Current == null)
            {
                MessageBox.Show("Нет участника");
                return;
            }

            MessageBox.Show("Расчёт риска пока не реализован");
        }

        private void GoHome()
        {
            ShowParticipant();
        }

        public ISeries[] RiskByUnitSeries { get; set; }
        public Axis[] UnitXAxis { get; set; }
        public Axis[] RiskYAxis { get; set; }

        public ISeries[] RecidivismSeries { get; set; }
        public Axis[] RecidivismXAxis { get; set; }

        public ISeries[] TopUnitsSeries { get; set; }
        public Axis[] TopUnitsXAxis { get; set; }

        public ISeries[] RiskDistributionSeries { get; set; }

        public ISeries[] PersonalRiskSeries { get; set; }
        public Axis[] DateXAxis { get; set; }
        public ObservableCollection<Participant> Participants { get; set; }
        public Participant SelectedParticipant { get; set; }
        public bool CanSave =>
            Current != null &&
            Current.IsValid();
    }
}