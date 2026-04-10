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

                var selectVm = new TestSelectionViewModel(defs, mode);

                selectVm.OnBack = () =>
                {
                    ShowParticipant();
                };

                if (mode == TestMode.Express)
                {
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
                }
                else
                {
                    selectVm.OnStart = (selectedDefs, m) =>
                    {
                        var multiVm = new MultiTestViewModel(this, selectedDefs, m);
                        CurrentView = new MultiTestView { DataContext = multiVm };
                    };
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

            if (!Current.IsValid())
            {
                MessageBox.Show("Исправьте ошибки");
                return;
            }

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

        public bool CanSave =>
            Current != null &&
            Current.IsValid();
    }
}