using System.Windows;
using System.Windows.Controls;
using PsyDiagnostics.ViewModels;

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
                vm.LoginPsychologist(PasswordBox.Password);
            }
        }
    }
}
