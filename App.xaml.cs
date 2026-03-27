using System.Windows;
using PsyDiagnostics.ViewModels;
using PsyDiagnostics.Services;

namespace PsyDiagnostics
{
    public partial class App : Application
    {
        // 🔥 ГЛОБАЛЬНЫЙ MainViewModel (для навигации)
        public static MainViewModel MainVM;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainVM = new MainViewModel();

            // 🖥️ запускаем окно
            var window = new MainWindow();
            window.DataContext = MainVM;
            window.Show();
        }
    }
}