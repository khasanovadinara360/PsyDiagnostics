using PsyDiagnostics.Models;
using PsyDiagnostics.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;

namespace PsyDiagnostics.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly DatabaseService db = new DatabaseService();
        private readonly ReportService reportService = new ReportService();
        private readonly RiskService riskService = new RiskService();

        // =========================
        // ПОИСК
        // =========================
        private string _searchId;
        public string SearchId
        {
            get => _searchId;
            set { _searchId = value; OnPropertyChanged(); }
        }

        // =========================
        // УЧАСТНИК
        // =========================
        private Participant _current;
        public Participant Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(); }
        }

        // =========================
        // РЕЗУЛЬТАТЫ
        // =========================
        public ObservableCollection<ResultRecord> Results { get; set; }

        // =========================
        // КОМАНДЫ
        // =========================
        public ICommand SearchCommand { get; }
        public ICommand ExportCommand { get; }

        public HistoryViewModel()
        {
            Results = new ObservableCollection<ResultRecord>();

            SearchCommand = new RelayCommand<object>(_ => Load());
            ExportCommand = new RelayCommand<object>(_ => Export());
        }

        // =========================
        // ЗАГРУЗКА ИЗ БД
        // =========================
        private void Load()
        {
            if (string.IsNullOrWhiteSpace(SearchId))
            {
                MessageBox.Show("Введите ID");
                return;
            }

            var (participant, results) = db.GetFullReport(SearchId);

            if (participant == null)
            {
                MessageBox.Show("Не найдено");
                return;
            }

            Current = participant;

            Results.Clear();
            foreach (var r in results)
                Results.Add(r);
        }

        // =========================
        // PDF + РИСК
        // =========================
        private void Export()
        {
            if (Current == null || Results.Count == 0)
            {
                MessageBox.Show("Нет данных");
                return;
            }

            // 👉 собираем словарь для ML
            var dict = Results.ToDictionary(r => r.TestName, r => r.Score);

            // 👉 считаем риск
            var risk = riskService.CalculateRisk(dict);

            // 👉 генерим PDF
            reportService.Generate(
                "report.pdf",
                Current,
                Results.ToList(),
                risk
            );

            MessageBox.Show("PDF сохранён 🔥");
        }
    }
}