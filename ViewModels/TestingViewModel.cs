using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using PsyDiagnostics.Services;

namespace PsyDiagnostics.ViewModels
{
    public class TestResultItem : BaseViewModel
    {
        public string TestName { get; set; }
        public int Score { get; set; }
        public string Risk { get; set; }
        public string Date { get; set; }
    }

    public class TestingViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new DatabaseService();

        public ObservableCollection<TestResultItem> Results { get; }

        public ICommand ExportPdfCommand { get; }

        public TestingViewModel(MainViewModel main)
        {
            _main = main;
            Results = new ObservableCollection<TestResultItem>();
            ExportPdfCommand = new RelayCommand(ExportPdf);

            LoadResults();
        }

        private void LoadResults()
        {
            Results.Clear();

            if (_main.Current == null)
                return;

            var report = _db.GetFullReport(_main.Current.PrisonerId);
            if (report.aiResults == null)
                return;

            foreach (var r in report.aiResults.OrderByDescending(r => r.Date))
            {
                string risk = r.Prediction == 1 ? "Высокий риск" : "Низкий риск";

                Results.Add(new TestResultItem
                {
                    TestName = r.TestName,
                    Score = r.Score,
                    Risk = risk,
                    Date = r.Date
                });
            }
        }

        private void ExportPdf()
        {
            // Заглушка: позже сюда добавим реальную генерацию PDF
            System.Windows.MessageBox.Show("Выгрузка PDF пока не реализована.");
        }
    }
}