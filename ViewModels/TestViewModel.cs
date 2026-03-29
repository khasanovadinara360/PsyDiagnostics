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
        //private readonly MlService _ml = new MlService();
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

            // обучение из БД (если есть данные)
           // _ml.TrainFromDatabase(db);
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

            string levelRisk =
                percent < 40 ? "Низкий" :
                percent < 70 ? "Средний" : "Высокий";

            string explanation = BuildExplanation(aiInput);

            if (_main.Current != null)
            {
                db.SaveTestResult(
                _main.Current.PrisonerId,
                _test.Name,
                finalScore,
                result, // вместо percent >= 50
                percent / 100.0
);
            }

            string level = _test.GetLevel(finalScore);

            MessageBox.Show(
                $"{_test.Name}\n" +
                $"Баллы: {finalScore}\n" +
                $"Уровень: {level}\n\n" +

                $"ИИ вероятность: {percent}%\n" +
                $"Риск: {levelRisk}\n\n" +

                $"Причины:\n{explanation}"
            );

            OnFinished?.Invoke();
        }

        private string BuildExplanation(AiData d)
        {
            var reasons = new List<string>();

            if (d.Aggression > 70)
                reasons.Add("Высокая агрессивность");

            if (d.Impulsivity > 70)
                reasons.Add("Высокая импульсивность");

            if (d.Stress < 40)
                reasons.Add("Низкая стрессоустойчивость");

            if (d.Adaptation < 40)
                reasons.Add("Плохая социальная адаптация");

            if (d.Anxiety > 70)
                reasons.Add("Высокая тревожность");

            if (d.Resilience < 40)
                reasons.Add("Низкая психологическая устойчивость");

            if (d.Hostility > 70)
                reasons.Add("Выраженная враждебность");

            return reasons.Count == 0
                ? "Факторы риска не выражены"
                : string.Join("\n", reasons);
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