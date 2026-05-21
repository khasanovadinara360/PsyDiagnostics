using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class MultiTestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly TestMode _mode;
        private readonly Dictionary<string, int> _allResults = new();

        private TestViewModel _currentTest;
        public TestViewModel CurrentTest
        {
            get => _currentTest;
            set { _currentTest = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TestViewModel> TestViewModels { get; } = new();

        public ICommand SelectTestCommand { get; }

        public MultiTestViewModel(
            MainViewModel main,
            IEnumerable<TestDefinition> definitions,
            TestMode mode)
        {
            _main = main;
            _mode = mode;

            CreateTestViewModels(definitions);

            CurrentTest = TestViewModels.FirstOrDefault();

            SelectTestCommand = new RelayCommand(parameter => SelectTest(parameter));
        }

        private void CreateTestViewModels(IEnumerable<TestDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                var viewModel = new TestViewModel(_main, definition, _mode);

                viewModel.OnFinished += () => OnSingleTestFinished(viewModel);

                TestViewModels.Add(viewModel);
            }
        }

        private void SelectTest(object parameter)
        {
            if (_mode != TestMode.Normal)
                return;

            if (parameter is TestViewModel viewModel)
                CurrentTest = viewModel;
        }

        private void OnSingleTestFinished(TestViewModel finished)
        {
            SaveTestResult(finished);

            finished.IsCompleted = true;

            if (TryMoveToNextFullTest(finished))
                return;

            if (AreAllTestsFinished())
            {
                _main.ShowResult(_allResults);
                return;
            }

            MessageBox.Show(
                "Вы выбрали несколько тестов. Чтобы получить результат, нужно пройти все выбранные тесты.");
        }

        private void SaveTestResult(TestViewModel finished)
        {
            var results = finished.GetResults();

            foreach (var result in results)
                _allResults[result.Key] = result.Value;
        }

        private bool TryMoveToNextFullTest(TestViewModel finished)
        {
            if (_mode != TestMode.Full)
                return false;

            int index = TestViewModels.IndexOf(finished);

            if (index < 0 || index >= TestViewModels.Count - 1)
                return false;

            CurrentTest = TestViewModels[index + 1];

            return true;
        }

        private bool AreAllTestsFinished()
        {
            return TestViewModels.All(viewModel =>
            {
                var results = viewModel.GetResults();

                return results != null && results.Count > 0;
            });
        }
    }
}