using PsyDiagnostics.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PsyDiagnostics.Views
{
    public partial class PsychologistLoginView : UserControl
    {
        private bool _passwordVisible;

        public PsychologistLoginView()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            string password = _passwordVisible
                ? VisiblePasswordBox.Text
                : PasswordBox.Password;

            viewModel.LoginPsychologist(password);
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (string.IsNullOrWhiteSpace(viewModel.PsychologistLoginFullName))
            {
                viewModel.LoginError = "Сначала введите логин";
                return;
            }

            var psychologist = viewModel.GetPsychologistByLogin(
                viewModel.PsychologistLoginFullName.Trim());

            if (psychologist == null)
            {
                viewModel.LoginError =
                    "Пользователь с таким логином не найден";

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
                Background = new SolidColorBrush(
                    Color.FromRgb(30, 30, 40)),

                Owner = Window.GetWindow(this)
            };

            bool? result = window.ShowDialog();

            if (result == true)
                viewModel.ResetPsychologistPassword(
                    resetView.NewPassword);
        }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                VisiblePasswordBox.Text = PasswordBox.Password;

                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordBox.Password = VisiblePasswordBox.Text;

                PasswordBox.Visibility = Visibility.Visible;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
            }

            TogglePasswordButton.Content = "👁";
        }
    }
}