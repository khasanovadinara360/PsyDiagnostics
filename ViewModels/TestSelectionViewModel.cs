using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class SelectableTestDefinition : BaseViewModel
    {
        private bool _isSelected;

        public TestDefinition Definition { get; }

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

        public SelectableTestDefinition(TestDefinition definition)
        {
            Definition = definition;
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

        public TestSelectionViewModel(
            IEnumerable<TestDefinition> definitions,
            TestMode mode)
        {
            Mode = mode;

            Tests = new ObservableCollection<SelectableTestDefinition>(
                definitions.Select(x => new SelectableTestDefinition(x)));

            SubscribeToSelectionChanges();

            StartCommand = new RelayCommand(_ => Start());

            BackCommand = new RelayCommand(_ => OnBack?.Invoke());
        }

        private void SubscribeToSelectionChanges()
        {
            foreach (var test in Tests)
                test.OnSelectionChanged += OnTestSelectionChanged;
        }

        private void OnTestSelectionChanged(
            SelectableTestDefinition changed,
            bool isSelected)
        {
            if (!isSelected || Mode != TestMode.Express)
                return;

            var selected = Tests
                .Where(x => x.IsSelected)
                .ToList();

            if (selected.Count <= 1)
            {
                UnselectOtherTests(changed);
                return;
            }

            changed.OnSelectionChanged -= OnTestSelectionChanged;

            changed.IsSelected = false;

            changed.OnSelectionChanged += OnTestSelectionChanged;

            MessageBox.Show(
                "В режиме экспресс можно выбрать только один тест.");
        }

        private void UnselectOtherTests(SelectableTestDefinition selected)
        {
            foreach (var test in Tests)
            {
                if (test == selected || !test.IsSelected)
                    continue;

                test.OnSelectionChanged -= OnTestSelectionChanged;

                test.IsSelected = false;

                test.OnSelectionChanged += OnTestSelectionChanged;
            }
        }

        private void Start()
        {
            var selectedTests = Tests
                .Where(x => x.IsSelected)
                .Select(x => x.Definition)
                .ToList();

            if (!ValidateSelection(selectedTests))
                return;

            OnStart?.Invoke(selectedTests, Mode);
        }

        private bool ValidateSelection(List<TestDefinition> selectedTests)
        {
            if (selectedTests.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один тест.");
                return false;
            }

            if (Mode == TestMode.Express)
            {
                if (selectedTests.Count > 1)
                {
                    MessageBox.Show(
                        "В режиме экспресс можно выбрать только один тест.");

                    return false;
                }

                return true;
            }

            if (Mode == TestMode.Normal)
            {
                if (selectedTests.Count < 2)
                {
                    MessageBox.Show(
                        "В обычном режиме нужно выбрать минимум два теста.");

                    return false;
                }

                if (selectedTests.Count > 7)
                {
                    MessageBox.Show(
                        "В обычном режиме можно выбрать максимум 7 тестов.");

                    return false;
                }
            }

            return true;
        }
    }
}