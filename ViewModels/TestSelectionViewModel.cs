using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.ViewModels
{
    public class SelectableTestDefinition : BaseViewModel
    {
        public TestDefinition Definition { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();

                OnSelectionChanged?.Invoke(this, value);
            }
        }

        public string DisplayName => Definition.DisplayName;

        public event Action<SelectableTestDefinition, bool> OnSelectionChanged;

        public SelectableTestDefinition(TestDefinition def)
        {
            Definition = def;
        }
    }

    public class TestSelectionViewModel : BaseViewModel
    {
        public ObservableCollection<SelectableTestDefinition> Tests { get; }

        public ICommand StartCommand { get; }
        public ICommand BackCommand { get; }

        public TestMode Mode { get; }

        public Action<IList<TestDefinition>, TestMode> OnStart { get; set; }
        public Action OnBack { get; set; }

        public TestSelectionViewModel(IEnumerable<TestDefinition> defs, TestMode mode)
        {
            Mode = mode;
            Tests = new ObservableCollection<SelectableTestDefinition>(
                defs.Select(d => new SelectableTestDefinition(d)));

            foreach (var t in Tests)
                t.OnSelectionChanged += OnTestSelectionChanged;

            StartCommand = new RelayCommand(Start);
            BackCommand = new RelayCommand(() => OnBack?.Invoke());
        }

        private void OnTestSelectionChanged(SelectableTestDefinition changed, bool isSelected)
        {
            if (!isSelected)
                return;

            if (Mode == TestMode.Express)
            {
                var alreadySelected = Tests.Where(t => t.IsSelected).ToList();

                if (alreadySelected.Count > 1)
                {
                    changed.OnSelectionChanged -= OnTestSelectionChanged;
                    changed.IsSelected = false;
                    changed.OnSelectionChanged += OnTestSelectionChanged;

                    MessageBox.Show("В режиме экспресс можно выбрать только один тест.");
                }
                else
                {
                    foreach (var t in Tests)
                    {
                        if (t != changed && t.IsSelected)
                        {
                            t.OnSelectionChanged -= OnTestSelectionChanged;
                            t.IsSelected = false;
                            t.OnSelectionChanged += OnTestSelectionChanged;
                        }
                    }
                }
            }
        }

        private void Start()
        {
            var selectedDefs = Tests
                .Where(t => t.IsSelected)
                .Select(t => t.Definition)
                .ToList();

            if (Mode == TestMode.Express)
            {
                if (selectedDefs.Count == 0)
                {
                    MessageBox.Show("Выберите один тест для экспресс‑режима.");
                    return;
                }

                if (selectedDefs.Count > 1)
                {
                    MessageBox.Show("В режиме экспресс можно выбрать только один тест.");
                    return;
                }
            }
            else if (Mode == TestMode.Normal)
            {
                if (selectedDefs.Count < 2)
                {
                    MessageBox.Show("В обычном режиме нужно выбрать минимум два теста.");
                    return;
                }
            }
            if (selectedDefs.Count == 0)
                return;

            OnStart?.Invoke(selectedDefs, Mode);
        }
    }
}