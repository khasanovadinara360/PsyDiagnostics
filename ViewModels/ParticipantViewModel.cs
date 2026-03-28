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

        // 🔥 чтобы биндинг работал нормально
        private string _searchId;
        public string SearchId
        {
            get => _searchId;
            set
            {
                _searchId = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoToTestCommand { get; }
        public ICommand SearchCommand { get; set; }

        public Action<Participant> OnNavigateToTest;

        public ParticipantViewModel()
        {
            GoToTestCommand = new RelayCommand(GoToTest);
        }

        private void GoToTest()
        {
            if (CurrentParticipant == null)
            {
                MessageBox.Show("Сначала сохраните или выберите участника");
                return;
            }

            // 🔥 ключевой момент — вызываем MainViewModel
            OnNavigateToTest?.Invoke(CurrentParticipant);
        }
    }
}