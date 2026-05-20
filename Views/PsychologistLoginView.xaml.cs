using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Views
{
    public partial class PsychologistLoginView : UserControl
    {
        private bool _passwordVisible = false;

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

            if (string.IsNullOrWhiteSpace(vm.PsychologistLoginFullName))
            {
                vm.LoginError = "Сначала введите логин";
                return;
            }

            var psychologist = vm.GetPsychologistByLogin(vm.PsychologistLoginFullName.Trim());

            if (psychologist == null)
            {
                vm.LoginError = "Пользователь с таким логином не найден";
                return;
            }

            var resetView = new ResetPasswordView();

            var window = new Window
            {
                Title = "Смена пароля",
                Content = resetView,
                Width = 420,
                Height = 320,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)),
                Owner = Window.GetWindow(this)
            };

            bool? result = window.ShowDialog();

            if (result == true)
            {
                vm.ResetPsychologistPassword(resetView.NewPassword);
            }
        }

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