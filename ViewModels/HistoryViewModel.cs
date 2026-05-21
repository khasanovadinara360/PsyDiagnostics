using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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
    public class HistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new();

        private Participant _participant;
        public Participant Participant
        {
            get => _participant;
            set { _participant = value; OnPropertyChanged(); }
        }

        private string _searchId;
        public string SearchId
        {
            get => _searchId;
            set { _searchId = value; OnPropertyChanged(); }
        }

        private ISeries[] _riskSeries;
        public ISeries[] RiskSeries
        {
            get => _riskSeries;
            set { _riskSeries = value; OnPropertyChanged(); }
        }

        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get => _xAxes;
            set { _xAxes = value; OnPropertyChanged(); }
        }

        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get => _yAxes;
            set { _yAxes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TestResultRecord> Results { get; } = new();

        public ICommand LoadCommand { get; }
        public ICommand BackCommand { get; }

        public HistoryViewModel(MainViewModel main)
        {
            _main = main;

            LoadCommand = new RelayCommand(_ => Load());
            BackCommand = new RelayCommand(_ => GoBack());
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
                MessageBox.Show("Участник не найден");
                return;
            }

            Participant = report.participant;

            Results.Clear();

            foreach (var result in report.aiResults)
                Results.Add(result);

            BuildChart(report.aiResults);
        }

        private void GoBack()
        {
            _main.ShowParticipantPage();
        }

        private void BuildChart(List<TestResultRecord> data)
        {
            var ordered = data
                .Where(x => DateTime.TryParse(x.Date, out _))
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
        }
    }
}