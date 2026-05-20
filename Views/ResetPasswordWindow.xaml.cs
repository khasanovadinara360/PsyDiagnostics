using System.Windows;

namespace PsyDiagnostics.Views
{
    public partial class ResetPasswordWindow : Window
    {
        public string NewPassword { get; private set; }

        public ResetPasswordWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            NewPassword = NewPasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}