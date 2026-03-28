using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Views
{
    public partial class TestView : System.Windows.Controls.UserControl
    {
        public TestView()
        {
            InitializeComponent();
            DataContext = new TestViewModel(new MainViewModel());
        }
    }
}