using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PsyDiagnostics.Views
{
    public partial class ResetPasswordView : UserControl
    {
        private bool _isPasswordVisible;

        public string NewPassword =>
            _isPasswordVisible
                ? NewPasswordTextBox.Text
                : NewPasswordBox.Password;

        public ResetPasswordView()
        {
            InitializeComponent();
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                NewPasswordTextBox.Text = NewPasswordBox.Password;

                NewPasswordTextBox.Visibility = Visibility.Visible;
                NewPasswordBox.Visibility = Visibility.Collapsed;

                EyeIcon.Text = "👁";

                NewPasswordTextBox.Focus();
                NewPasswordTextBox.CaretIndex = NewPasswordTextBox.Text.Length;
            }
            else
            {
                NewPasswordBox.Password = NewPasswordTextBox.Text;

                NewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordTextBox.Visibility = Visibility.Collapsed;

                EyeIcon.Text = "👁";

                NewPasswordBox.Focus();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var password = NewPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите новый пароль");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Пароль должен содержать минимум 8 символов");
                return;
            }

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
            {
                ShowError("Пароль должен содержать хотя бы одну заглавную букву");
                return;
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                ShowError("Пароль должен содержать хотя бы одну цифру");
                return;
            }

            if (!Regex.IsMatch(password, @"[^\w\s]"))
            {
                ShowError("Пароль должен содержать хотя бы один спецсимвол");
                return;
            }
            ResetPasswordErrorText.Text = "";

            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }

        private void ShowError(string message)
        {
            ResetPasswordErrorText.Foreground =
                new SolidColorBrush(Color.FromRgb(255, 83, 112));

            ResetPasswordErrorText.Text = message;
        }
    }
}