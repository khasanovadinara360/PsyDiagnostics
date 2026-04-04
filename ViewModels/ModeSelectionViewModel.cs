using System;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.ViewModels;

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

            ExpressCommand = new RelayCommand(() => OnModeSelected?.Invoke(TestMode.Express));
            NormalCommand = new RelayCommand(() => OnModeSelected?.Invoke(TestMode.Normal));
            FullCommand = new RelayCommand(() => OnModeSelected?.Invoke(TestMode.Full));
            BackCommand = new RelayCommand(() => OnBack?.Invoke());
            HomeCommand = new RelayCommand(() => _main.GoHomeCommand.Execute(null));
        }
    }
}