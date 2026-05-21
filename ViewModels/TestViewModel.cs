using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new();
        private readonly ApiService _api = new();

        private readonly Test _test;
        private readonly Dictionary<string, int> _results = new();

        private int _currentIndex;
        private bool _isCompleted;

        public Action OnFinished { get; set; }

        public ObservableCollection<Question> Questions { get; }

        public Participant SelectedParticipant { get; set; }

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged();

                UpdateQuestionState();
            }
        }

        public int CurrentIndex => _currentIndex;

        public int TotalQuestions => Questions.Count;

        public string QuestionNumber =>
            $"{_currentIndex + 1}/{TotalQuestions}";

        public string NextButtonText =>
            _currentIndex == TotalQuestions - 1
                ? "Завершить"
                : "Далее";

        public Visibility FinishButtonVisibility =>
            _currentIndex == TotalQuestions - 1
                ? Visibility.Visible
                : Visibility.Collapsed;

        public string TestTitle =>
            _test.DisplayName ?? _test.Name;

        public string TestTitleWithCheck =>
            IsCompleted
                ? $"✓ {TestTitle}"
                : TestTitle;

        private string _modeTitle;
        public string ModeTitle
        {
            get => _modeTitle;
            set { _modeTitle = value; OnPropertyChanged(); }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(TestTitleWithCheck));
            }
        }

        public ICommand NextCommand { get; }

        public ICommand PrevCommand { get; }

        public ICommand FinishCommand { get; }

        public TestViewModel(
            MainViewModel main,
            TestDefinition definition,
            TestMode mode)
        {
            _main = main;

            _test = LoadTest(definition);

            ConfigureQuestions();

            Questions = new ObservableCollection<Question>(_test.Questions);

            CurrentQuestion = Questions.FirstOrDefault();

            ModeTitle = GetModeTitle(mode);

            NextCommand = new RelayCommand(_ => Next());

            PrevCommand = new RelayCommand(
                _ => Prev(),
                _ => _currentIndex > 0);

            FinishCommand = new RelayCommand(
                async _ => await FinishTest());
        }

        private Test LoadTest(TestDefinition definition)
        {
            var tests = TestLoader.LoadTests();

            return tests.First(x => x.Name == definition.Name);
        }

        private void ConfigureQuestions()
        {
            foreach (var question in _test.Questions)
            {
                question.TestViewModel = this;

                foreach (var answer in question.Answers)
                {
                    answer.Question = question;
                    answer.TestViewModel = this;
                }
            }
        }

        private string GetModeTitle(TestMode mode)
        {
            return mode switch
            {
                TestMode.Express => "Формат: экспресс-тест",
                TestMode.Normal => "Формат: обычный тест",
                TestMode.Full => "Формат: расширенный тест",
                _ => "Формат: неизвестный"
            };
        }

        private void Next()
        {
            if (!ValidateCurrentAnswer())
                return;

            SaveCurrentAnswer();

            MoveNext();
        }

        private void Prev()
        {
            if (_currentIndex <= 0)
                return;

            _currentIndex--;

            CurrentQuestion = Questions[_currentIndex];

            RestoreAnswerSelection();
        }

        private bool ValidateCurrentAnswer()
        {
            return CurrentQuestion.Answers.Any(x => x.IsSelected)
                || ShowSelectAnswerMessage();
        }

        private bool ShowSelectAnswerMessage()
        {
            MessageBox.Show("Выберите ответ");

            return false;
        }

        private void SaveCurrentAnswer()
        {
            var selected = CurrentQuestion.Answers
                .First(x => x.IsSelected);

            CurrentQuestion.Answer = selected.Value;
        }

        private void MoveNext()
        {
            _currentIndex++;

            if (_currentIndex >= Questions.Count)
            {
                _ = FinishTest();
                return;
            }

            CurrentQuestion = Questions[_currentIndex];

            RestoreAnswerSelection();
        }

        private void RestoreAnswerSelection()
        {
            foreach (var answer in CurrentQuestion.Answers)
            {
                answer.IsSelected =
                    answer.Value == CurrentQuestion.Answer;
            }
        }

        public void OnAnswerSelected()
        {
            if (_currentIndex >= Questions.Count - 1)
            {
                UpdateQuestionState();
                return;
            }

            MoveNext();
        }

        public Dictionary<string, int> GetResults()
        {
            return _results;
        }

        private void UpdateQuestionState()
        {
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(TotalQuestions));
            OnPropertyChanged(nameof(QuestionNumber));
            OnPropertyChanged(nameof(NextButtonText));
            OnPropertyChanged(nameof(FinishButtonVisibility));
        }

        private async Task FinishTest()
        {
            int finalScore = CalculateFinalScore();

            _results[_test.Name] = finalScore;

            var request = BuildPredictionRequest();

            await SaveAiPrediction(request);

            OnFinished?.Invoke();

            _main.UpdateUnitRisk();
        }

        private int CalculateFinalScore()
        {
            int total = Questions.Sum(x => x.Answer);

            int maxValue = Questions
                .SelectMany(x => x.Answers)
                .Max(x => x.Value);

            int maxScore = Questions.Count * maxValue;

            return (int)((total / (double)maxScore) * 100);
        }

        private PredictionRequest BuildPredictionRequest()
        {
            return new PredictionRequest
            {
                Aggression = _results.GetValueOrDefault("Aggression", 50),
                Impulsivity = _results.GetValueOrDefault("Impulsivity", 50),
                Depression = _results.GetValueOrDefault("Depression", 50),
                Stress = _results.GetValueOrDefault("Stress", 50),
                Adaptation = _results.GetValueOrDefault("Adaptation", 50),
                Anxiety = _results.GetValueOrDefault("Anxiety", 50),
                Resilience = _results.GetValueOrDefault("Resilience", 50),
                Hostility = _results.GetValueOrDefault("Hostility", 50)
            };
        }

        private async Task SaveAiPrediction(PredictionRequest request)
        {
            try
            {
                int prediction = await _api.GetPrediction(request);

                double probability =
                    prediction == 1 ? 0.8 : 0.2;

                int score = (int)(probability * 100);

                if (SelectedParticipant == null)
                    return;

                _db.SaveTestResult(
                    SelectedParticipant.PrisonerId,
                    SelectedParticipant.Unit,
                    "AI",
                    score,
                    prediction,
                    probability);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка ИИ: " + ex.Message);
            }
        }
    }
}