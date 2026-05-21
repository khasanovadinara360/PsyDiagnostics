using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;

namespace PsyDiagnostics.ViewModels
{
    public class ResultsViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new();

        public ObservableCollection<TestResultRecord> Results { get; } = new();

        public ResultsViewModel(string prisonerId)
        {
            LoadResults(prisonerId);
        }

        private void LoadResults(string prisonerId)
        {
            var report = _db.GetFullReport(prisonerId);

            Results.Clear();

            foreach (var result in report.aiResults)
                Results.Add(result);
        }
    }
}