using PsyDiagnostics.ViewModels;
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

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.S)
                return;

            if (DataContext is MainViewModel viewModel)
                viewModel.SaveCommand.Execute(null);

            e.Handled = true;
        }

        private void ComboBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
                comboBox.IsDropDownOpen = true;
        }
    }
}