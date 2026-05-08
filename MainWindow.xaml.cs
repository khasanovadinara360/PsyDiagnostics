using System;
using System.Windows;
using System.Windows.Input;
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

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Maximize_Click(sender, e);
                return;
            }

            DragMove();
        }

        public void SetParticipantTitle(string fullName)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.TopBarTitle = GetShortName(fullName);
            }
        }

        private string GetShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "PsyDiagnostics";

            var parts = fullName.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return fullName;

            return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
        }
    }
}