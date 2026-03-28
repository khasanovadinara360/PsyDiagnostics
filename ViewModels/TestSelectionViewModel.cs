using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.ViewModels
{
    public class TestSelectionViewModel : BaseViewModel
    {
        public ObservableCollection<TestDefinition> Tests { get; set; }

        private TestDefinition _selectedTest;
        public TestDefinition SelectedTest
        {
            get => _selectedTest;
            set
            {
                _selectedTest = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectCommand { get; }

        public event Action<TestDefinition> OnTestSelected;

        public TestSelectionViewModel()
        {
            // 🔥 Русские названия (должны совпадать с JSON!)
            Tests = new ObservableCollection<TestDefinition>
            {
                new TestDefinition { Name = "Aggression" },
                new TestDefinition { Name = "Impulsivity" },
                new TestDefinition { Name = "Depression" },
                new TestDefinition { Name = "Stress" },
                new TestDefinition { Name = "Adaptation" },
                new TestDefinition { Name = "Anxiety" },
                new TestDefinition { Name = "Resilience" },
                new TestDefinition { Name = "Hostility" }
            };

            SelectCommand = new RelayCommand(obj =>
            {
                if (obj is TestDefinition test)
                {
                   
                    OnTestSelected?.Invoke(test);
                }
            });
        }
    }
}