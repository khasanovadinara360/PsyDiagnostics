using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PsyDiagnostics.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new DatabaseService();
        private Participant _participant;
        public Participant Participant
        {
            get => _participant;
            set
            {
                _participant = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TestResultRecord> Results
    = new ObservableCollection<TestResultRecord>();

        public ICommand LoadCommand { get; }
        public ICommand BackCommand { get; }

        private string _searchId;
        public string SearchId
        {
            get => _searchId;
            set
            {
                _searchId = value;
                OnPropertyChanged();
            }
        }
        public HistoryViewModel(MainViewModel main)
        {
            _main = main;

            LoadCommand = new RelayCommand(() => Load());
            BackCommand = new RelayCommand(() => GoBack());
        }
        private void Load()
        {
            if (string.IsNullOrWhiteSpace(SearchId))
            {
                MessageBox.Show("Введите ID");
                return;
            }

            var report = _db.GetFullReport(SearchId);

            if (report.participant == null)
            {
                MessageBox.Show("Не найден");
                return;
            }

            Participant = report.participant;

            Results.Clear();
            foreach (var r in report.aiResults)
                Results.Add(r);
            BuildChart(report.aiResults);
        }
        private void GoBack()
        {
            _main.CurrentView = _main;
        }
        public ISeries[] RiskSeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        private void BuildChart(List<TestResultRecord> data)
        {
            var ordered = data
                .OrderBy(x => DateTime.Parse(x.Date))
                .ToList();

            RiskSeries = new ISeries[]
            {
        new LineSeries<double>
        {
            Name = "Риск",
            Values = ordered
                .Select(x => Math.Clamp(x.RiskScore, 0, 100))
                .ToArray()
        }
            };

            XAxes = new Axis[]
            {
        new Axis
        {
            Labels = ordered
                .Select(x => DateTime.Parse(x.Date).ToShortDateString())
                .ToArray()
        }
            };

            YAxes = new Axis[]
            {
        new Axis
        {
            MinLimit = 0,
            MaxLimit = 100
        }
            };

            OnPropertyChanged(nameof(RiskSeries));
            OnPropertyChanged(nameof(XAxes));
            OnPropertyChanged(nameof(YAxes));
        }
    }
}