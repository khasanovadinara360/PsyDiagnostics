using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.ViewModels;

namespace PsyDiagnostics.Models
{
    public class Answer : BaseViewModel
    {
        private bool _isSelected;

        public string Text { get; set; }
        public int Value { get; set; }

        public Question Question { get; set; }
        public TestViewModel TestViewModel { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectAnswerCommand { get; }

        public Answer()
        {
            SelectAnswerCommand = new RelayCommand(() =>
            {
                if (Question == null || TestViewModel == null)
                    return;

                foreach (var a in Question.Answers)
                    a.IsSelected = a == this;

                Question.Answer = Value;

                TestViewModel.OnAnswerSelected();
            });
        }
    }
}
