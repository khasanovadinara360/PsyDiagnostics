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
        }

        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void OnlyLetters(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[а-яА-Я]+$");
        }
    }
}