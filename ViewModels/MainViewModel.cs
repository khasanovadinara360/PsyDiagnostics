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
using LiveChartsCore.Painting;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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

        private string _topBarTitle = "PsyDiagnostics";


        private string _psychologistFullName;
        public string PsychologistFullName
        {
            get => _psychologistFullName;
            set
            {
                _psychologistFullName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TopBarTitle));
            }
        }

        private string _psychologistLoginFullName;
        public string PsychologistLoginFullName
        {
            get => _psychologistLoginFullName;
            set
            {
                _psychologistLoginFullName = value;
                OnPropertyChanged();
            }
        }

        private string _loginError;
        public string LoginError
        {
            get => _loginError;
            set
            {
                _loginError = value;
                OnPropertyChanged();
            }
        }

        public string TopBarTitle
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PsychologistFullName))
                    return $"Психолог: {PsychologistFullName}";

                return _topBarTitle;
            }
            set
            {
                _topBarTitle = value;
                OnPropertyChanged();
            }
        }
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
                TopBarTitle = _current != null
    ? GetShortName(_current.FullName)
    : "PsyDiagnostics";

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

        private UserRole _currentRole = UserRole.Psychologist;

        public UserRole CurrentRole
        {
            get => _currentRole;
            set
            {
                _currentRole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPsychologist));
                OnPropertyChanged(nameof(IsPrisoner));
                OnPropertyChanged(nameof(PsychologistVisibility));
                OnPropertyChanged(nameof(PrisonerVisibility));
            }
        }

        private void AddTest()
        {
            var vm = new AddTestViewModel(this);
            CurrentView = new AddTestView
            {
                DataContext = vm
            };
        }

        private string GetShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "PsyDiagnostics";

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return fullName;

            return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
        }

        public bool IsPsychologist => CurrentRole == UserRole.Psychologist;
        public bool IsPrisoner => CurrentRole == UserRole.Prisoner;

        public Visibility PsychologistVisibility =>
            IsPsychologist ? Visibility.Visible : Visibility.Collapsed;

        public Visibility PrisonerVisibility =>
            IsPrisoner ? Visibility.Visible : Visibility.Collapsed;

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

        public ICommand SearchCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoToTestCommand { get; }
        public ICommand CalculateRiskCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand ExportPdfCommand { get; }
        public ICommand SelectPsychologistRoleCommand { get; }
        public ICommand SelectPrisonerRoleCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand PrisonerStartTestCommand { get; }
        public ICommand ExtendedSearchCommand { get; }
        public ICommand ClearSearchFiltersCommand { get; }

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


        private string _searchFio;
        public string SearchFio
        {
            get => _searchFio;
            set
            {
                _searchFio = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                if (!string.IsNullOrWhiteSpace(_searchFio) && _searchFio.Length >= 2)
                    ExtendedSearch();
                else
                    SearchResults.Clear();
            }
        }

        private string _filterCountry;
        public string FilterCountry
        {
            get => _filterCountry;
            set
            {
                _filterCountry = value;
                OnPropertyChanged();
            }
        }

        private string _filterCity = "Не выбрано";
        public string FilterCity
        {
            get => _filterCity;
            set
            {
                _filterCity = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _filterArticle = "Не выбрано";
        public string FilterArticle
        {
            get => _filterArticle;
            set
            {
                _filterArticle = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _ageFromText = "Не выбрано";
        public string AgeFromText
        {
            get => _ageFromText;
            set
            {
                _ageFromText = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _ageToText = "Не выбрано";
        public string AgeToText
        {
            get => _ageToText;
            set
            {
                _ageToText = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _sentenceFromNumberText = "Не выбрано";
        public string SentenceFromNumberText
        {
            get => _sentenceFromNumberText;
            set
            {
                _sentenceFromNumberText = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _sentenceFromUnit = "Не выбрано";
        public string SentenceFromUnit
        {
            get => _sentenceFromUnit;
            set
            {
                _sentenceFromUnit = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _sentenceToNumberText = "Не выбрано";
        public string SentenceToNumberText
        {
            get => _sentenceToNumberText;
            set
            {
                _sentenceToNumberText = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _sentenceToUnit = "Не выбрано";
        public string SentenceToUnit
        {
            get => _sentenceToUnit;
            set
            {
                _sentenceToUnit = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private ObservableCollection<ParticipantSearchResult> _searchResults
            = new ObservableCollection<ParticipantSearchResult>();

        public ObservableCollection<ParticipantSearchResult> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }


        private ParticipantSearchResult _selectedSearchResult;
        public ParticipantSearchResult SelectedSearchResult
        {
            get => _selectedSearchResult;
            set
            {
                _selectedSearchResult = value;
                OnPropertyChanged();

                if (_selectedSearchResult == null)
                    return;

                _isFillingSearchFields = true;

                SearchId = _selectedSearchResult.PrisonerId;
                SearchFio = _selectedSearchResult.FullName;
                FilterCitizenship = _selectedSearchResult.Citizenship;
                FilterCity = _selectedSearchResult.Residence;
                FilterArticle = _selectedSearchResult.ArticleNumber;
                AgeFromText = "16";
                AgeToText = _selectedSearchResult.Age.ToString();

                // В БД срок хранится в годах, поэтому при выборе строки
                // ставим диапазон: от 2 месяцев → до срока осужденного

                SentenceFromNumberText = _selectedSearchResult.SentenceTerm > 0
                    ? "2"
                    : "Не выбрано";

                SentenceFromUnit = _selectedSearchResult.SentenceTerm > 0
                    ? "месяцев"
                    : "Не выбрано";

                SentenceToNumberText = _selectedSearchResult.SentenceTerm > 0
                    ? _selectedSearchResult.SentenceTerm.ToString()
                    : "Не выбрано";

                SentenceToUnit = _selectedSearchResult.SentenceTerm > 0
                    ? "лет"
                    : "Не выбрано";

                FilterUnit = string.IsNullOrWhiteSpace(_selectedSearchResult.Unit)
                    ? "Не выбрано"
                    : _selectedSearchResult.Unit;

                FilterRisk = string.IsNullOrWhiteSpace(_selectedSearchResult.Risk)
                    ? "Не выбрано"
                    : _selectedSearchResult.Risk;

                _isFillingSearchFields = false;

                SearchCommand.Execute(null);
            }
        }

        private Visibility _searchResultsVisibility = Visibility.Collapsed;
        public Visibility SearchResultsVisibility
        {
            get => _searchResultsVisibility;
            set
            {
                _searchResultsVisibility = value;
                OnPropertyChanged();
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
        public Array CitizenshipValues => Enum.GetValues(typeof(Citizenship));
        public ObservableCollection<string> Cities { get; set; } = new();
        public ObservableCollection<string> Articles { get; set; } = new();
        public ObservableCollection<object> SentenceValues { get; set; } = new();
        public ObservableCollection<string> Units { get; set; } = new();
        public ObservableCollection<object> AgeValues { get; set; } = new();
        public ObservableCollection<object> TermNumberValues { get; set; } = new();

        public ObservableCollection<string> TermUnitValues { get; set; } = new()
{
    "Не выбрано",
    "месяцев",
    "лет"
};
        public ObservableCollection<string> RiskValues { get; set; } = new()
{
    "Не выбрано",
    "Низкий",
    "Средний",
    "Высокий"
};

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
            SelectPsychologistRoleCommand = new RelayCommand(_ =>
            {
                SelectRole(UserRole.Psychologist);
            });

            SelectPrisonerRoleCommand = new RelayCommand(_ =>
            {
                SelectRole(UserRole.Prisoner);
            });
            ExtendedSearchCommand = new RelayCommand(ExtendedSearch);
            ClearSearchFiltersCommand = new RelayCommand(ClearSearchFilters);
            PrisonerStartTestCommand = new RelayCommand(_ => PrisonerStartTest());
            AddTestCommand = new RelayCommand(_ => AddTest());
            AllArticles = JsonHelper.LoadArticles();
            FilteredArticles = AllArticles;


            _isInitializingFilters = true;
            LoadSearchFilters();
            LoadSentenceValues();

            FilterCitizenship = Citizenship.НеВыбрано;
            FilterCity = "Не выбрано";
            FilterArticle = "Не выбрано";
            FilterUnit = "Не выбрано";
            FilterRisk = "Не выбрано";
            AgeFromText = "Не выбрано";
            AgeToText = "Не выбрано";
            SentenceFromNumberText = "Не выбрано";
            SentenceFromUnit = "Не выбрано";
            SentenceToNumberText = "Не выбрано";
            SentenceToUnit = "Не выбрано";

            _isInitializingFilters = false;

            if (Units.Any())
                SelectedUnit = Units.First();

            ShowRoleSelection();

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


        private void LoadSearchFilters()
        {
            Cities = new ObservableCollection<string>();
            Cities.Add("Не выбрано");

            foreach (var city in _db.GetDistinctValues("Residence"))
                Cities.Add(city);

            Articles = new ObservableCollection<string>();
            Articles.Add("Не выбрано");

            foreach (var article in _db.GetDistinctValues("ArticleNumber"))
                Articles.Add(article);

            Units = new ObservableCollection<string>();
            Units.Add("Не выбрано");

            foreach (var unit in _db.GetDistinctValues("Unit"))
                Units.Add(unit);

            OnPropertyChanged(nameof(Units));
            OnPropertyChanged(nameof(Cities));
            OnPropertyChanged(nameof(Articles));
        }

        private bool _isInitializingFilters;
        private void LoadSentenceValues()
        {
            AgeValues.Clear();
            AgeValues.Add("Не выбрано");

            for (int i = 16; i <= 100; i++)
                AgeValues.Add(i.ToString());

            TermNumberValues.Clear();
            TermNumberValues.Add("Не выбрано");

            // Срок можно вводить вручную, а список даёт быстрый выбор от 1 до 35.
            for (int i = 1; i <= 35; i++)
                TermNumberValues.Add(i.ToString());

            OnPropertyChanged(nameof(AgeValues));
            OnPropertyChanged(nameof(TermNumberValues));
        }

        private string _filterUnit = "Не выбрано";
        public string FilterUnit
        {
            get => _filterUnit;
            set
            {
                _filterUnit = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private string _filterRisk = "Не выбрано";
        public string FilterRisk
        {
            get => _filterRisk;
            set
            {
                _filterRisk = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }


        private string _searchMessage;
        public string SearchMessage
        {
            get => _searchMessage;
            set
            {
                _searchMessage = value;
                OnPropertyChanged();
            }
        }

        private void ShowParticipant()
        {
            try
            {
                using var db = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=psy.db");
                db.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка подключения к базе данных SQLite.\n\n" + ex.Message,
                    "Ошибка БД",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }

            ParticipantVm.OnNavigateToTest = participant =>
            {
                Current = participant;
                GoToTest();
            };

            CurrentView = new ParticipantView
            {
                DataContext = this
            };
        }

        public void ShowParticipantPage()
        {
            ShowParticipant();
        }

        private void ExtendedSearch()
        {
            var city = IsEmptyFilter(FilterCity)
                ? null
                : FilterCity;

            var article = IsEmptyFilter(FilterArticle)
                ? null
                : FilterArticle;

            var unit = IsEmptyFilter(FilterUnit)
                ? null
                : FilterUnit;

            var risk = IsEmptyFilter(FilterRisk)
                ? null
                : FilterRisk;

            int? ageFrom = ParseNullableInt(AgeFromText);
            int? ageTo = ParseNullableInt(AgeToText);

            int? sentenceFrom = GetTermInMonths(SentenceFromNumberText, SentenceFromUnit);
            int? sentenceTo = GetTermInMonths(SentenceToNumberText, SentenceToUnit);

            var results = _db.SearchParticipants(
                SearchFio,
                FilterCitizenship,
                city,
                ageFrom,
                ageTo,
                article,
                sentenceFrom,
                sentenceTo,
                unit,
                risk
            );

            SearchResults = new ObservableCollection<ParticipantSearchResult>(results);

            SearchMessage = SearchResults.Count == 0
                ? "Пользователь с такими данными не найден"
                : string.Empty;
        }

        private bool IsEmptyFilter(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value == "Не выбрано";
        }

        private int? ParseNullableInt(string value)
        {
            if (IsEmptyFilter(value))
                return null;

            return int.TryParse(value, out var number)
                ? number
                : null;
        }

        private int? GetTermInMonths(string numberText, string unitText)
        {
            if (!int.TryParse(numberText, out var number))
                return null;

            if (unitText == "месяцев")
                return number;

            if (unitText == "лет")
                return number * 12;

            return null;
        }


        private void ClearSearchFilters()
        {
            _isFillingSearchFields = true;

            SearchId = string.Empty;
            SearchFio = string.Empty;

            FilterCitizenship = Citizenship.НеВыбрано;
            FilterCity = "Не выбрано";
            FilterArticle = "Не выбрано";
            FilterUnit = "Не выбрано";
            FilterRisk = "Не выбрано";

            AgeFromText = "16";
            AgeToText = "36";

            SentenceFromNumberText = "Не выбрано";
            SentenceFromUnit = "Не выбрано";
            SentenceToNumberText = "Не выбрано";
            SentenceToUnit = "Не выбрано";

            SelectedSearchResult = null;

            SearchResults.Clear();
            SearchMessage = string.Empty;

            ParticipantVm.CurrentParticipant = new Participant();

            _isFillingSearchFields = false;
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
            try
            {
                if (Current == null)
                {
                    MessageBox.Show("Нет участника");
                    return;
                }

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show("Нет результатов тестирования для сохранения");
                    return;
                }

                SaveResultsToDatabase(results);

                LoadTestHistory();
                BuildPersonalChart();
                BuildPersonalAiSummary();
                UpdateChart();
                BuildRiskByUnitsChart();
                BuildRecidivismChart();
                BuildTopUnitsChart();

                string text = "РЕЗУЛЬТАТЫ СОХРАНЕНЫ:\n\n";

                foreach (var r in results)
                {
                    string risk = GetRiskTextByScore(r.Value);
                    text += $"{TranslateTestName(r.Key)}\n" +
                            $"Баллы: {r.Value}\n" +
                            $"Уровень: {risk}\n\n";
                }

                MessageBox.Show(text);

                CanGoHomeAfterTests = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения результатов тестирования:\n" + ex.Message);
            }
        }

        private void SaveResultsToDatabase(Dictionary<string, int> results)
        {
            double aggression = GetResultScore(results, "Aggression");
            double impulsivity = GetResultScore(results, "Impulsivity");
            double depression = GetResultScore(results, "Depression");
            double stress = GetResultScore(results, "Stress");
            double adaptation = GetResultScore(results, "Adaptation");
            double anxiety = GetResultScore(results, "Anxiety");
            double resilience = GetResultScore(results, "Resilience");
            double hostility = GetResultScore(results, "Hostility");

            foreach (var item in results)
            {
                int score = item.Value;
                double probability = Math.Max(0, Math.Min(1, score / 30.0));
                int prediction = score >= 21 ? 1 : 0;

                _db.SaveTestResult(
                    Current.PrisonerId?.ToString(),
                    Current.Unit,
                    item.Key,
                    score,
                    prediction,
                    probability,
                    aggression,
                    impulsivity,
                    depression,
                    stress,
                    adaptation,
                    anxiety,
                    resilience,
                    hostility
                );
            }
        }


        private double GetResultScore(Dictionary<string, int> results, string testName)
        {
            return results != null && results.TryGetValue(testName, out int value)
                ? value
                : 0;
        }

        private string GetRiskTextByScore(int score)
        {
            if (score <= 10)
                return "Низкий риск";

            if (score <= 20)
                return "Средний риск";

            return "Высокий риск";
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

        private string TranslateTestName(string name)
        {
            switch (name)
            {
                case "Aggression": return "Агрессивность";
                case "Impulsivity": return "Импульсивность";
                case "Depression": return "Депрессивное состояние";
                case "Stress": return "Стрессоустойчивость";
                case "Adaptation": return "Социальная адаптация";
                case "Anxiety": return "Тревожность";
                case "Resilience": return "Психологическая устойчивость";
                case "Hostility": return "Враждебность";
                default: return name;
            }
        }

        private void ExportPdf()
        {
            try
            {
                if (Current == null)
                {
                    MessageBox.Show("Нет участника");
                    return;
                }

                LoadTestHistory();

                PdfReportService.GenerateTestingReport(
                    Current,
                    TestHistory,
                    UnitRisk
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при выгрузке PDF:\n" + ex.Message);
            }
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

        private Citizenship _filterCitizenship = Citizenship.НеВыбрано;
        public Citizenship FilterCitizenship
        {
            get => _filterCitizenship;
            set
            {
                _filterCitizenship = value;
                OnPropertyChanged();

                if (_isInitializingFilters || _isFillingSearchFields)
                    return;

                ExtendedSearch();
            }
        }

        private bool _isFillingSearchFields;

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

        private void PrisonerStartTest()
        {
            var id = SearchId?.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show(
                    "Введите ID заключённого",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var participant = _db.GetParticipant(id);

            if (participant == null)
            {
                MessageBox.Show(
                    $"ID {id} не найден в системе.",
                    "Заключённый не найден",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            Current = participant;

            GoToTest();
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
                var query = SearchId?.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    MessageBox.Show("Введите ID или ФИО");
                    return;
                }

                // Проверяем, является ли запрос ID (цифры)
                bool isIdSearch = int.TryParse(query, out _);
                Participant found;

                if (isIdSearch)
                {
                    // Точный поиск по ID
                    found = _db.GetParticipant(query);
                }
                else
                {
                    // Поиск по ФИО (LIKE '%query%')
                    found = _db.GetParticipantByName(query);
                }

                if (found == null)
                {
                    Current = new Participant
                    {
                        PrisonerId = query,
                        BirthDate = DateTime.Today
                    };

                    MessageBox.Show($"Не найден: {query}");
                    return;
                }

                Current = found;

                var article = AllArticles
                    .FirstOrDefault(a => a.Number?.Trim() == Current.ArticleNumber?.Trim());

                SelectedArticle = article;

                if (article != null)
                {
                    ArticleSearch = article.Number;
                    FilteredArticles = new List<Article> { article };
                }
                else
                {
                    ArticleSearch = "";
                    FilteredArticles = AllArticles;
                }

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

        private void ShowRoleSelection()
        {
            CurrentView = new RoleSelectionView { DataContext = this };
        }

        private void SelectRole(UserRole role)
        {
            CurrentRole = role;

            if (role == UserRole.Psychologist)
            {
                PsychologistLoginFullName = "";
                LoginError = "";

                CurrentView = new PsychologistLoginView
                {
                    DataContext = this
                };

                return;
            }

            ShowParticipant();
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

        public void LoginPsychologist(string password)
        {
            LoginError = "";

            if (string.IsNullOrWhiteSpace(PsychologistLoginFullName))
            {
                LoginError = "Введите ФИО";
                _db.AddPsychologistLoginLog("Не указано", false, "Попытка входа без ФИО");
                return;
            }

            if (password != "12")
            {
                LoginError = "Неверный пароль";
                _db.AddPsychologistLoginLog(PsychologistLoginFullName.Trim(), false, "Неверный пароль");
                return;
            }

            PsychologistFullName = PsychologistLoginFullName.Trim();
            _db.AddPsychologistLoginLog(PsychologistFullName, true, "Успешный вход");
            TopBarTitle = $"Психолог: {PsychologistFullName}";

            ShowParticipant();
        }


    }
}
