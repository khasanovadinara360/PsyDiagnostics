using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Views
{
    public partial class HistoryView : System.Windows.Controls.UserControl
    {
        public HistoryView()
        {
            InitializeComponent();
            DataContext = new HistoryViewModel();
        }
    }
}