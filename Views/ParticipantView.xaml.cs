using System.Windows.Controls;
using System.Windows.Input;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Views
{
    public partial class ParticipantView : UserControl
    {
        public ParticipantView()
        {
            InitializeComponent();
            // НЕ устанавливаем DataContext здесь
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control &&
                e.Key == Key.S)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.SaveCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        private void ComboBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = true;
            }
        }
    }
}