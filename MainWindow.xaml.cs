using System.Windows;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}