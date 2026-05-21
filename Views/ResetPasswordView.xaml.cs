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
                ShowPasswordText();
            else
                ShowPasswordBox();

            EyeIcon.Text = "👁";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string password = NewPassword;

            if (!ValidatePassword(password))
                return;

            ResetPasswordErrorText.Text = "";

            var window = Window.GetWindow(this);

            if (window == null)
                return;

            window.DialogResult = true;
            window.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window == null)
                return;

            window.DialogResult = false;
            window.Close();
        }

        private void ShowPasswordText()
        {
            NewPasswordTextBox.Text = NewPasswordBox.Password;
            NewPasswordTextBox.Visibility = Visibility.Visible;
            NewPasswordBox.Visibility = Visibility.Collapsed;

            NewPasswordTextBox.Focus();
            NewPasswordTextBox.CaretIndex = NewPasswordTextBox.Text.Length;
        }

        private void ShowPasswordBox()
        {
            NewPasswordBox.Password = NewPasswordTextBox.Text;
            NewPasswordBox.Visibility = Visibility.Visible;
            NewPasswordTextBox.Visibility = Visibility.Collapsed;

            NewPasswordBox.Focus();
        }

        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ShowError("Введите новый пароль");

            if (password.Length < 8)
                return ShowError("Пароль должен содержать минимум 8 символов");

            if (!Regex.IsMatch(password, @"[A-ZА-Я]"))
                return ShowError("Пароль должен содержать хотя бы одну заглавную букву");

            if (!Regex.IsMatch(password, @"\d"))
                return ShowError("Пароль должен содержать хотя бы одну цифру");

            if (!Regex.IsMatch(password, @"[^\w\s]"))
                return ShowError("Пароль должен содержать хотя бы один спецсимвол");

            return true;
        }

        private bool ShowError(string message)
        {
            ResetPasswordErrorText.Foreground =
                new SolidColorBrush(Color.FromRgb(255, 83, 112));

            ResetPasswordErrorText.Text = message;

            return false;
        }
    }
}