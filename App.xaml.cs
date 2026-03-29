using System.Windows;
using PsyDiagnostics.ViewModels;
using PsyDiagnostics.Services;

namespace PsyDiagnostics
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ВРЕМЕННО (один запуск)
           // ModelTrainer.Train();
        }
    }
}