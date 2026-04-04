using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;

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

        public ObservableCollection<ResultRecord> Results { get; set; }
            = new ObservableCollection<ResultRecord>();

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
            foreach (var r in report.results)
                Results.Add(r);
        }
        private void GoBack()
        {
            _main.CurrentView = _main;
        }
    }
}