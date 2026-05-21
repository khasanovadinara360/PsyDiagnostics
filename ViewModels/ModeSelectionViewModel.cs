using PsyDiagnostics.Helpers;
using System;
using System.Windows.Input;

namespace PsyDiagnostics.ViewModels
{
    public class ModeSelectionViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ICommand ExpressCommand { get; }

        public ICommand NormalCommand { get; }

        public ICommand FullCommand { get; }

        public ICommand BackCommand { get; }

        public ICommand HomeCommand { get; }

        public Action<TestMode> OnModeSelected { get; set; }

        public Action OnBack { get; set; }

        public ModeSelectionViewModel(MainViewModel main)
        {
            _main = main;

            ExpressCommand =
                new RelayCommand(_ => SelectMode(TestMode.Express));

            NormalCommand =
                new RelayCommand(_ => SelectMode(TestMode.Normal));

            FullCommand =
                new RelayCommand(_ => SelectMode(TestMode.Full));

            BackCommand =
                new RelayCommand(_ => OnBack?.Invoke());

            HomeCommand =
                new RelayCommand(_ => _main.GoHomeCommand.Execute(null));
        }

        private void SelectMode(TestMode mode)
        {
            OnModeSelected?.Invoke(mode);
        }
    }
}