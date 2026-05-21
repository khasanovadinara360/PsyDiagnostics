using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace PsyDiagnostics.ViewModels
{
    public class TestResultItem : BaseViewModel
    {
        public string TestName { get; set; } = string.Empty;

        public int Score { get; set; }

        public string Risk { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;
    }

    public class TestingViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly DatabaseService _db = new();

        public ObservableCollection<TestResultItem> Results { get; } = new();

        public TestingViewModel(MainViewModel main)
        {
            _main = main;

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

            foreach (var result in report.aiResults.OrderByDescending(x => x.Date))
            {
                Results.Add(new TestResultItem
                {
                    TestName = result.TestName,
                    Score = result.Score,
                    Risk = result.Prediction == 1 ? "Высокий риск" : "Низкий риск",
                    Date = result.Date
                });
            }
        }
    }
}