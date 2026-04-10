using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService db = new DatabaseService();
        private readonly ApiService _api = new ApiService();

        public ObservableCollection<Question> Questions { get; set; }

        private int _currentIndex;
        public Action OnFinished { get; set; }

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged();
            }
        }

        private Test _test;
        private Dictionary<string, int> results = new();

        public string TestTitle => _test.DisplayName ?? _test.Name;

        private string _modeTitle;
        public string ModeTitle
        {
            get => _modeTitle;
            set { _modeTitle = value; OnPropertyChanged(); }
        }

        public int CurrentIndex => _currentIndex;
        public int TotalQuestions => Questions?.Count ?? 0;
        public string QuestionNumber => $"{_currentIndex + 1}/{TotalQuestions}";
        public string NextButtonText =>
            _currentIndex == TotalQuestions - 1 ? "Завершить" : "Далее";

        public System.Windows.Visibility FinishButtonVisibility =>
            _currentIndex == TotalQuestions - 1
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand FinishCommand { get; }

        public TestViewModel(MainViewModel main, TestDefinition selectedTest, TestMode mode)
        {
            _main = main;

            var service = new TestService();
            var allTests = service.LoadTests();

            _test = allTests.First(t => t.Name == selectedTest.Name);

            ModeTitle = mode switch
            {
                TestMode.Express => "Формат: экспресс‑тест",
                TestMode.Normal => "Формат: обычный тест",
                TestMode.Full => "Формат: расширенный тест",
                _ => "Формат: неизвестный"
            };

            foreach (var q in _test.Questions)
            {
                q.TestViewModel = this;

                foreach (var a in q.Answers)
                {
                    a.Question = q;
                    a.TestViewModel = this;
                }
            }

            Questions = new ObservableCollection<Question>(_test.Questions);
            CurrentQuestion = Questions.FirstOrDefault();

            NextCommand = new RelayCommand(Next);
            PrevCommand = new RelayCommand(Prev, () => _currentIndex > 0);
            FinishCommand = new RelayCommand(() => FinishTest());
        }

        private void Next()
        {
            var selected = CurrentQuestion.Answers
                .FirstOrDefault(a => a.IsSelected);

            if (selected == null)
            {
                MessageBox.Show("Выберите ответ");
                return;
            }

            CurrentQuestion.Answer = selected.Value;

            _currentIndex++;

            if (_currentIndex < Questions.Count)
            {
                CurrentQuestion = Questions[_currentIndex];

                foreach (var a in CurrentQuestion.Answers)
                    a.IsSelected = a.Value == CurrentQuestion.Answer;

                RaiseAll();
            }
            else
            {
                FinishTest();
            }
        }

        private void Prev()
        {
            if (_currentIndex <= 0)
                return;

            _currentIndex--;

            CurrentQuestion = Questions[_currentIndex];

            foreach (var a in CurrentQuestion.Answers)
                a.IsSelected = a.Value == CurrentQuestion.Answer;

            RaiseAll();
        }

        private async void FinishTest()
        {
            int sum = Questions.Sum(q => q.Answer);

            int maxPerQuestion = Questions
                .SelectMany(q => q.Answers ?? new List<Answer>())
                .Max(a => a.Value);

            int rawScore = sum;
            int maxScore = Questions.Count * maxPerQuestion;

            int finalScore = (int)((rawScore / (double)maxScore) * 100);

            results[_test.Name] = finalScore;

            var aiInput = new AiData
            {
                Aggression = results.GetValueOrDefault("Aggression", 50),
                Impulsivity = results.GetValueOrDefault("Impulsivity", 50),
                Depression = results.GetValueOrDefault("Depression", 50),
                Stress = results.GetValueOrDefault("Stress", 50),
                Adaptation = results.GetValueOrDefault("Adaptation", 50),
                Anxiety = results.GetValueOrDefault("Anxiety", 50),
                Resilience = results.GetValueOrDefault("Resilience", 50),
                Hostility = results.GetValueOrDefault("Hostility", 50)
            };

            var request = new PredictionRequest
            {
                Aggression = aiInput.Aggression,
                Impulsivity = aiInput.Impulsivity,
                Depression = aiInput.Depression,
                Stress = aiInput.Stress,
                Adaptation = aiInput.Adaptation,
                Anxiety = aiInput.Anxiety,
                Resilience = aiInput.Resilience,
                Hostility = aiInput.Hostility
            };

            int result = await _api.GetPrediction(request);

            int percent = result == 1 ? 80 : 20;

            if (_main.Current != null)
            {
                db.SaveTestResult(
                    _main.Current.PrisonerId,
                    _test.Name,
                    finalScore,
                    result,
                    percent / 100.0
                );
            }

            // здесь больше не показываем общий MessageBox,
            // только даём сигнал MultiTestViewModel / MainViewModel
            OnFinished?.Invoke();
        }

        public Dictionary<string, int> GetResults() => results;

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(TotalQuestions));
            OnPropertyChanged(nameof(QuestionNumber));
            OnPropertyChanged(nameof(NextButtonText));
            OnPropertyChanged(nameof(FinishButtonVisibility));
        }

        public void OnAnswerSelected()
        {
            if (_currentIndex >= Questions.Count - 1)
            {
                RaiseAll();
                return;
            }

            _currentIndex++;

            if (_currentIndex < Questions.Count)
            {
                CurrentQuestion = Questions[_currentIndex];

                foreach (var a in CurrentQuestion.Answers)
                    a.IsSelected = a.Value == CurrentQuestion.Answer;

                RaiseAll();
            }
        }
    }
}