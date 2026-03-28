using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private DatabaseService _db = new DatabaseService();

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

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
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
                    FilteredArticles = null;
                else
                    FilteredArticles = AllArticles
                        .Where(a => a.Number.Contains(value) || a.Title.ToLower().Contains(value.ToLower()))
                        .ToList();
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

                if (value != null && Current != null)
                {
                    Current.ArticleNumber = value.Number;
                    Current.ArticlePart = value.Parts?.FirstOrDefault();
                    Current.ArticlePoint = value.Points?.FirstOrDefault();

                    OnPropertyChanged(nameof(AvailableParts));
                    OnPropertyChanged(nameof(AvailablePoints));
                }
            }
        }

        public List<string> AvailableParts => SelectedArticle?.Parts;
        public List<string> AvailablePoints => SelectedArticle?.Points;

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(Search);
            SaveCommand = new RelayCommand(Save);
            GoToTestCommand = new RelayCommand(GoToTest);
            CalculateRiskCommand = new RelayCommand(CalculateRisk);

            AllArticles = JsonHelper.LoadArticles();

            ShowParticipant();
        }

        private void ShowParticipant()
        {
            var vm = new ParticipantViewModel();

            vm.CurrentParticipant = Current;

            vm.OnNavigateToTest = (participant) =>
            {
                Current = participant;
                GoToTest();
            };

            CurrentView = vm;
        }

        private void GoToTest()
        {
            if (Current == null)
            {
                MessageBox.Show("Сначала найдите участника");
                return;
            }

            var selectionVM = new TestSelectionViewModel();

            selectionVM.OnTestSelected += (selectedTest) =>
            {
                var testVM = new TestViewModel(this, selectedTest);

                testVM.OnFinished += () =>
                {
                    ShowResult(testVM.GetResults());
                };

                CurrentView = testVM;
            };

            CurrentView = selectionVM;
        }

        // 🔥 ВОТ ГЛАВНОЕ — ВЫВОД РЕЗУЛЬТАТОВ
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
        }

        public void ShowHistory()
        {
            CurrentView = new ParticipantViewModel();
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

                if (found != null)
                {
                    Current = found;

                    if (CurrentView is ParticipantViewModel vm)
                        vm.CurrentParticipant = found;

                    MessageBox.Show("Найден ✔");
                }
                else
                {
                    var newP = new Participant
                    {
                        PrisonerId = id,
                        BirthDate = DateTime.Today
                    };

                    Current = newP;

                    if (CurrentView is ParticipantViewModel vm)
                        vm.CurrentParticipant = newP;

                    MessageBox.Show("Не найден");
                }
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

        public bool CanSave =>
            Current != null &&
            Current.IsValid();
    }
}