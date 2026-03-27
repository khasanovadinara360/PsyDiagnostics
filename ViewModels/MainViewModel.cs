using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public object CurrentView { get; set; }

        public Participant CurrentParticipant { get; set; }

        public ICommand CalculateRiskCommand { get; }

        public MainViewModel()
        {
            ShowParticipant();

            CalculateRiskCommand = new RelayCommand(() =>
            {
                System.Windows.MessageBox.Show("ИИ анализ выполнен 🔥");
            });
        }

        public void ShowParticipant()
        {
            CurrentView = new ParticipantViewModel(this);
            OnPropertyChanged(nameof(CurrentView));
        }

        public void ShowTest()
        {
            CurrentView = new TestViewModel(this);
            OnPropertyChanged(nameof(CurrentView));
        }

        public void ShowResult(Dictionary<string, int> results)
        {
            CurrentView = new ResultViewModel(this, results);
            OnPropertyChanged(nameof(CurrentView));
        }

        public void ShowHistory()
        {
            CurrentView = new HistoryViewModel();
            OnPropertyChanged(nameof(CurrentView));
        }
    }
}