using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.ViewModels
{
    public class TestSelectionViewModel
    {
        public ObservableCollection<TestDefinition> Tests { get; set; }

        public TestDefinition SelectedTest { get; set; }

        public ICommand SelectCommand { get; }

        public event Action<TestDefinition> OnTestSelected;

        public TestSelectionViewModel()
        {
            Tests = new ObservableCollection<TestDefinition>
            {
                new TestDefinition { Name = "Агрессия", QuestionCount = 10 },
                new TestDefinition { Name = "Импульсивность", QuestionCount = 10 }
            };

            SelectCommand = new RelayCommand<TestDefinition>(t =>
            {
                SelectedTest = t;
                OnTestSelected?.Invoke(t);
            });
        }
    }
}