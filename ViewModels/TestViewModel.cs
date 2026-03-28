using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using System;

namespace PsyDiagnostics.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService db = new DatabaseService();

        private int _currentIndex;
        private int _currentTestIndex;

        public ObservableCollection<Test> Tests { get; set; }
        public ObservableCollection<Question> Questions { get; set; }

        private Question _currentQuestion;
        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set { _currentQuestion = value; OnPropertyChanged(); }
        }

        public string TestName => Tests[_currentTestIndex].Name;

        public int Progress => Questions.Count == 0 ? 0 :
            (_currentIndex + 1) * 100 / Questions.Count;

        public ICommand AnswerCommand { get; }
        public ICommand NextTestCommand { get; }
        public ICommand FinishCommand { get; }

        public event Action OnFinished;

        // 👉 ВСЕ РЕЗУЛЬТАТЫ
        private Dictionary<string, int> allResults = new();

        // =========================
        // КОНСТРУКТОР (ОДИН!)
        // =========================
        public TestViewModel(MainViewModel main)
        {
            _main = main;

            var service = new TestService();
            Tests = new ObservableCollection<Test>(service.LoadTests());

            LoadTest(0);

            AnswerCommand = new RelayCommand<int>(Answer);
            NextTestCommand = new RelayCommand(() => NextTest());

            FinishCommand = new RelayCommand(() =>
            {
                OnFinished?.Invoke();
            });
        }

        // =========================
        // ЗАГРУЗКА ТЕСТА
        // =========================
        private void LoadTest(int index)
        {
            _currentTestIndex = index;
            _currentIndex = 0;

            Questions = new ObservableCollection<Question>(Tests[index].Questions);

            CurrentQuestion = Questions.FirstOrDefault();

            OnPropertyChanged(nameof(TestName));
            OnPropertyChanged(nameof(Progress));
        }

        // =========================
        // ОТВЕТ
        // =========================
        private void Answer(int value)
        {
            if (CurrentQuestion == null)
                return;

            CurrentQuestion.Answer = value;

            _currentIndex++;

            if (_currentIndex < Questions.Count)
            {
                CurrentQuestion = Questions[_currentIndex];
                OnPropertyChanged(nameof(Progress));
            }
            else
            {
                FinishTest();
            }
        }

        // =========================
        // ЗАВЕРШЕНИЕ ТЕСТА
        // =========================
        private void FinishTest()
        {
            var test = Tests[_currentTestIndex];

            int sum = Questions.Sum(q => q.Answer);

            string result;

            if (sum <= test.LowMax)
                result = "Низкий";
            else if (sum <= test.MediumMax)
                result = "Средний";
            else
                result = "Высокий";

            // 💾 Сохраняем в БД
            if (_main.Current != null)
            {
                db.SaveResult(
                    _main.Current.PrisonerId,
                    test.Name,
                    sum
                );
            }

            // сохраняем результат
            allResults[test.Name] = sum;

            MessageBox.Show($"{test.Name}\nБаллы: {sum}\n{result}");
        }

        // =========================
        // СЛЕДУЮЩИЙ ТЕСТ
        // =========================
        private void NextTest()
        {
            if (_currentTestIndex < Tests.Count - 1)
            {
                LoadTest(_currentTestIndex + 1);
            }
            else
            {
                // 👉 ВСЕ ТЕСТЫ ПРОЙДЕНЫ
                _main.ShowResult(allResults);

                OnFinished?.Invoke();
            }
        }
    }
}