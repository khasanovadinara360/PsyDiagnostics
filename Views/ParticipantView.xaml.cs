using PsyDiagnostics.ViewModels;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace PsyDiagnostics.Views
{
    public partial class ParticipantView : UserControl
    {
        public ParticipantView()
        {
            InitializeComponent();
            DataContext = new ParticipantViewModel(App.MainVM);
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void OnlyLetters(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[а-яА-Яa-zA-Z]+$");
        }
    }
}