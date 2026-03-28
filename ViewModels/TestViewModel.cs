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

        public string TestTitle => _test.Name;
        public int CurrentIndex => _currentIndex;
        public int TotalQuestions => Questions?.Count ?? 0;
        public string QuestionNumber => $"{_currentIndex + 1}/{TotalQuestions}";

        public string NextButtonText =>
            _currentIndex == TotalQuestions - 1 ? "Завершить" : "Далее";

        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }

        private Dictionary<string, int> results = new();
        private Test _test;

        // 🔥 КОНСТРУКТОР С ВЫБРАННЫМ ТЕСТОМ
        public TestViewModel(MainViewModel main, TestDefinition selectedTest)
        {
            _main = main;

            var service = new TestService();
            var allTests = service.LoadTests();

            _test = allTests.First(t => t.Name == selectedTest.Name);

            Questions = new ObservableCollection<Question>(_test.Questions);
            CurrentQuestion = Questions.FirstOrDefault();

            NextCommand = new RelayCommand(Next);
            PrevCommand = new RelayCommand(Prev, () => _currentIndex > 0);
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

                // 🔥 восстановление выбора
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

        private void FinishTest()
        {
            int sum = Questions.Sum(q => q.Answer);

            int maxPerQuestion = Questions
                .SelectMany(q => q.Answers ?? new List<Answer>())
                .Max(a => a.Value);

            double normalized = (double)sum / (Questions.Count * maxPerQuestion) * 100;
            int finalScore = (int)normalized;

            string level = _test.GetLevel(finalScore);

            if (_main.Current != null)
            {
                db.SaveResult(
                    _main.Current.PrisonerId,
                    _test.Name,
                    finalScore
                );
            }

            results[_test.Name] = finalScore;

            MessageBox.Show($"{_test.Name}\nБаллы: {finalScore}\nУровень: {level}");

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
        }
    }
}