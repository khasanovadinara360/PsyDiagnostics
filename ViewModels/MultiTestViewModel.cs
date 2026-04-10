using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.ViewModels
{
    public class MultiTestViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly TestMode _mode;

        public ObservableCollection<TestViewModel> TestViewModels { get; }

        private TestViewModel _currentTest;
        public TestViewModel CurrentTest
        {
            get => _currentTest;
            set { _currentTest = value; OnPropertyChanged(); }
        }

        // общий словарь результатов по всем тестам
        private readonly Dictionary<string, int> _allResults = new Dictionary<string, int>();

        public ICommand SelectTestCommand { get; }

        public MultiTestViewModel(MainViewModel main,
                                  IEnumerable<TestDefinition> defs,
                                  TestMode mode)
        {
            _main = main;
            _mode = mode;

            TestViewModels = new ObservableCollection<TestViewModel>();

            foreach (var def in defs)
            {
                var tvm = new TestViewModel(_main, def, mode);
                tvm.OnFinished += () => OnSingleTestFinished(tvm);
                TestViewModels.Add(tvm);
            }

            CurrentTest = TestViewModels.FirstOrDefault();

            SelectTestCommand = new RelayCommand(p => SelectTest(p));
        }

        private void SelectTest(object parameter)
        {
            var vm = parameter as TestViewModel;
            if (vm == null)
                return;

            if (_mode == TestMode.Normal)
            {
                CurrentTest = vm;
            }
            // в Full порядок фиксированный — игнорируем выбор
        }

        private void OnSingleTestFinished(TestViewModel finished)
        {
            var res = finished.GetResults();
            foreach (var kv in res)
                _allResults[kv.Key] = kv.Value;

            if (_mode == TestMode.Full)
            {
                var idx = TestViewModels.IndexOf(finished);
                if (idx >= 0 && idx < TestViewModels.Count - 1)
                {
                    CurrentTest = TestViewModels[idx + 1];
                    return;
                }
            }
            bool allFinished = TestViewModels.All(vm =>
            {
                var r = vm.GetResults();
                return r != null && r.Count > 0;
            });

            if (allFinished)
            {
                _main.ShowResult(_allResults);
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Вы выбрали несколько тестов. " +
                    "Чтобы получить результат, нужно пройти все выбранные тесты."
                );
            }
        }
    }
}