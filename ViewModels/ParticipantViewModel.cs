using System;
using System.Windows;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;

namespace PsyDiagnostics.ViewModels
{
    public class ParticipantViewModel : BaseViewModel
    {
        private Participant _currentParticipant;

        public Participant CurrentParticipant
        {
            get => _currentParticipant;
            set
            {
                _currentParticipant = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoToTestCommand { get; }
        public ICommand SearchCommand { get; set; }
        public string SearchId { get; set; }

        public Action<Participant> OnNavigateToTest;


        private void GoToTest()
        {
            if (CurrentParticipant == null)
            {
                MessageBox.Show("Сначала сохраните или выберите участника");
                return;
            }

            OnNavigateToTest?.Invoke(CurrentParticipant);
        }
        public ParticipantViewModel()
        {
            GoToTestCommand = new RelayCommand(() => GoToTest());
        }
    }
}