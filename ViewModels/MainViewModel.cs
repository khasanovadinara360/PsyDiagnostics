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
using System.Windows.Input;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
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
    }

    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new DatabaseService();

        // VM анкеты участника
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
                SelectedArticle = AllArticles
                .FirstOrDefault(a =>
                    a.Number != null &&
                    a.Number.Trim() == _current?.ArticleNumber?.Trim());
                if (_current != null)
                    _current.PropertyChanged -= Current_PropertyChanged;

                _current = value;

                if (_current != null)
                    _current.PropertyChanged += Current_PropertyChanged;

                ParticipantVm.CurrentParticipant = _current;
                SelectedArticle = AllArticles
        .FirstOrDefault(a => a.Number?.Trim() == _current?.ArticleNumber?.Trim());

                OnPropertyChanged(nameof(SelectedArticle));
                OnPropertyChanged(nameof(CanSave));
                UpdateUnitRisk();
                LoadTestHistory();

                //OnPropertyChanged();
                //OnPropertyChanged(nameof(CanSave));

                //LoadTestHistory();
            }
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
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
            //changes

            ShowParticipant();
        }

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

            CanGoHomeAfterTests = true;
        }

        private void LoadTestHistory()
        {
            TestHistory.Clear();

            if (Current == null)
                return;

            var report = _db.GetFullReport(Current.PrisonerId);

            if (report.aiResults == null)
                return;

            foreach (var r in report.aiResults.OrderByDescending(x => x.Date))
            {
                string risk = r.Prediction == 1 ? "Высокий риск" : "Низкий риск";

                TestHistory.Add(new TestHistoryItem
                {
                    TestName = r.TestName,
                    Score = r.Score,
                    Risk = risk,
                    Date = r.Date
                });
            }
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

            // 🔥 PIE С ПОДПИСЯМИ
            int total = UnitCount;

            RiskDistributionSeries = new ISeries[]
            {
    new PieSeries<double>
    {
        Values = new List<double> { low },
        Name = "Низкий",
        DataLabelsFormatter = p =>
        {
            double percent = total == 0 ? 0 : (p.Model / total) * 100;
            return $"{p.Model} чел ({percent:F0}%)";
        }
    },
    new PieSeries<double>
    {
        Values = new List<double> { mid },
        Name = "Средний",
        DataLabelsFormatter = p =>
        {
            double percent = total == 0 ? 0 : (p.Model / total) * 100;
            return $"{p.Model} чел ({percent:F0}%)";
        }
    },
    new PieSeries<double>
    {
        Values = new List<double> { high },
        Name = "Высокий",
        DataLabelsFormatter = p =>
        {
            double percent = total == 0 ? 0 : (p.Model / total) * 100;
            return $"{p.Model} чел ({percent:F0}%)";
        }
    }
            };

            BuildRiskByUnitsChart();
            BuildRecidivismChart();
            BuildTopUnitsChart();
            BuildPersonalChart();

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

            OnPropertyChanged(nameof(RiskByUnitSeries));
            OnPropertyChanged(nameof(UnitXAxis));
        }
        public void BuildRecidivismChart()
        {
            var data = _db.GetRecidivismStats();

            // 🔥 приводим к double (важно)
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
                return $"{(int)value} чел ({percent:F0}%)";
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

            RiskYAxis = new Axis[]
            {
        new Axis
        {
            MinLimit = 0
        }
            };

            OnPropertyChanged(nameof(RecidivismSeries));
            OnPropertyChanged(nameof(RecidivismXAxis));
            OnPropertyChanged(nameof(RiskYAxis));
        }
        public void BuildTopUnitsChart()
        {
            var data = _db.GetTopUnitsImprovement();

            if (data == null || data.Count == 0)
                return;

            // 🔥 считаем общее значение
            double total = data.Sum(x => x.improvement);

            TopUnitsSeries = new ISeries[]
            {
        new ColumnSeries<double>
        {
            Values = data.Select(x => (double)x.improvement).ToArray(),

            DataLabelsFormatter = point =>
            {
                double value = point.Coordinate.PrimaryValue;
                double percent = total == 0 ? 0 : (value / total) * 100;

                return $"{(int)value} чел ({percent:F0}%)";
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

            RiskYAxis = new Axis[]
            {
        new Axis
        {
            MinLimit = 0
        }
            };

            OnPropertyChanged(nameof(TopUnitsSeries));
            OnPropertyChanged(nameof(TopUnitsXAxis));
            OnPropertyChanged(nameof(RiskYAxis));
        }
        public void BuildPersonalChart()
        {
            var report = _db.GetFullReport(Current.PrisonerId);

            var ordered = report.aiResults
                .OrderBy(x => DateTime.Parse(x.Date))
                .ToList();

            PersonalRiskSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Values = ordered.Select(x => x.RiskScore).ToArray(),
            DataLabelsFormatter = p => $"{p.Model:F0}%"
        }
            };

            DateXAxis = new Axis[]
            {
        new Axis
        {
            Labels = ordered
                .Select(x => DateTime.Parse(x.Date).ToShortDateString())
                .ToArray()
        }
            };

            OnPropertyChanged(nameof(PersonalRiskSeries));
            OnPropertyChanged(nameof(DateXAxis));
        }


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
            //if (!Current.IsValid())
            //{
            //    MessageBox.Show("Исправьте ошибки");
            //    return;
            //}

            _db.SaveParticipant(Current);
            MessageBox.Show("Сохранено");
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