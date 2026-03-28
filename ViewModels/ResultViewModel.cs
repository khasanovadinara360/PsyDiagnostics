using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;

namespace PsyDiagnostics.ViewModels
{
    public class ResultsViewModel : BaseViewModel
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ObservableCollection<TestResultRecord> Results { get; set; }
            = new ObservableCollection<TestResultRecord>();

        public ResultsViewModel(string prisonerId)
        {
            Load(prisonerId);
        }

        private void Load(string id)
        {
            var data = _db.GetFullReport(id);

            Results.Clear();

            foreach (var r in data.aiResults)
                Results.Add(r);
        }
    }
}