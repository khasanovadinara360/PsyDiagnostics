using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using PsyDiagnostics.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class ParticipantViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService db = new DatabaseService();
        private readonly ArticleService articleService = new ArticleService();

        // =========================
        // УЧАСТНИК
        // =========================
        private Participant _current = new Participant();
        public Participant Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged();
            }
        }

        // =========================
        // ПОИСК
        // =========================
        public string SearchId { get; set; }

        public ICommand SearchCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand GoToTestCommand { get; }

        // =========================
        // СПРАВОЧНИКИ
        // =========================
        public ObservableCollection<string> Citizenships { get; } =
            new() { "Россия", "Казахстан", "Другое" };

        public ObservableCollection<string> EducationLevels { get; } =
            new() { "Начальное", "Среднее", "Высшее" };

        public ObservableCollection<string> MaritalStatuses { get; } =
            new() { "Холост", "Женат", "Разведён" };

        public ObservableCollection<string> CrimeTypes { get; } =
            new() { "Насильственное", "Имущественное", "Экономическое" };

        public ObservableCollection<string> Recidivisms { get; } =
            new() { "Нет", "Есть" };

        public ObservableCollection<string> Categories { get; } =
            new() { "Общий режим", "Строгий режим" };

        // =========================
        // СТАТЬИ
        // =========================
        private ObservableCollection<Article> _articles = new();
        public ObservableCollection<Article> Articles
        {
            get => _articles;
            set
            {
                _articles = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Article> _filteredArticles = new();
        public ObservableCollection<Article> FilteredArticles
        {
            get => _filteredArticles;
            set
            {
                _filteredArticles = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _availableParts = new();
        public ObservableCollection<string> AvailableParts
        {
            get => _availableParts;
            set
            {
                _availableParts = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _availablePoints = new();
        public ObservableCollection<string> AvailablePoints
        {
            get => _availablePoints;
            set
            {
                _availablePoints = value;
                OnPropertyChanged();
            }
        }

        private string _articleSearch;
        public string ArticleSearch
        {
            get => _articleSearch;
            set
            {
                _articleSearch = value;
                FilterArticles();
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

                if (value != null)
                {
                    Current.CriminalArticle = value.Display;

                    AvailableParts = new ObservableCollection<string>(value.Parts ?? new());
                    AvailablePoints = new ObservableCollection<string>(value.Points ?? new());
                }

                OnPropertyChanged();
            }
        }

        // =========================
        // КОНСТРУКТОР
        // =========================
        public ParticipantViewModel(MainViewModel main)
        {
            _main = main;

            LoadArticles();

            SearchCommand = new RelayCommand(Search);
            SaveCommand = new RelayCommand(Save);
            GoToTestCommand = new RelayCommand(GoToTest);
        }

        // =========================
        // JSON
        // =========================
        private void LoadArticles()
        {
            try
            {
                var list = articleService.Load();

                Articles = new ObservableCollection<Article>(list);
                FilteredArticles = new ObservableCollection<Article>(list);
            }
            catch
            {
                MessageBox.Show("Ошибка загрузки articles.json");
            }
        }

        private void FilterArticles()
        {
            if (string.IsNullOrWhiteSpace(ArticleSearch))
            {
                FilteredArticles = new ObservableCollection<Article>(Articles);
            }
            else
            {
                var text = ArticleSearch.ToLower();

                FilteredArticles = new ObservableCollection<Article>(
                    Articles.Where(a =>
                        a.Number.Contains(text) ||
                        (a.Title?.ToLower().Contains(text) == true))
                );
            }
        }

        // =========================
        // БД
        // =========================
        private void Search()
        {
            var p = db.GetParticipant(SearchId);

            if (p == null)
            {
                MessageBox.Show("Не найдено");
                return;
            }

            Current = p;
        }

        private void Save()
        {
            db.SaveParticipant(Current);
            MessageBox.Show("Сохранено");
        }

        // =========================
        // НАВИГАЦИЯ
        // =========================
        private void GoToTest()
        {
            _main.CurrentParticipant = Current;
            _main.ShowTest();
        }
    }
}