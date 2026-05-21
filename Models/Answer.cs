using PsyDiagnostics.Helpers;
using PsyDiagnostics.ViewModels;
using Newtonsoft.Json;

namespace PsyDiagnostics.Models
{
    public class Answer : BaseViewModel
    {
        private bool _isSelected;

        public string Text { get; set; } = string.Empty;

        public int Value { get; set; }

        [JsonIgnore]
        public Question Question { get; set; }

        [JsonIgnore]
        public TestViewModel TestViewModel { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SelectAnswerCommand { get; }

        public Answer()
        {
            SelectAnswerCommand = new RelayCommand(_ =>
            {
                if (Question == null || TestViewModel == null)
                    return;

                foreach (var answer in Question.Answers)
                    answer.IsSelected = answer == this;

                Question.Answer = Value;

                TestViewModel.OnAnswerSelected();
            });
        }
    }
}