using System.Windows;
using System.Windows.Controls;
using PsyDiagnostics.ViewModels;
using Microsoft.VisualBasic;

namespace PsyDiagnostics.Views
{
    public partial class PsychologistLoginView : UserControl
    {
        public PsychologistLoginView()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var password = _passwordVisible
                    ? VisiblePasswordBox.Text
                    : PasswordBox.Password;

                vm.LoginPsychologist(password);
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
                return;

            // Сначала проверяем логин
            if (string.IsNullOrWhiteSpace(vm.PsychologistLoginFullName))
            {
                vm.LoginError = "Сначала введите логин";
                return;
            }

            // Проверяем существование логина
            var psychologist =
                vm.GetPsychologistByLogin(vm.PsychologistLoginFullName.Trim());

            if (psychologist == null)
            {
                vm.LoginError = "Пользователь с таким логином не найден";
                return;
            }

            // Только после этого открываем окно смены пароля
            var window = new ResetPasswordWindow();

            if (window.ShowDialog() == true)
            {
                vm.ResetPsychologistPassword(window.NewPassword);
            }
        }

        private bool _passwordVisible = false;

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (!_passwordVisible)
            {
                VisiblePasswordBox.Text = PasswordBox.Password;

                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;

                TogglePasswordButton.Content = "👁";

                _passwordVisible = true;
            }
            else
            {
                PasswordBox.Password = VisiblePasswordBox.Text;

                PasswordBox.Visibility = Visibility.Visible;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;

                TogglePasswordButton.Content = "👁";

                _passwordVisible = false;
            }
        }
    }
}