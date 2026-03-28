using System;
using System.Collections.Generic;
using System.Windows.Input;
using PsyDiagnostics.Helpers;
using PsyDiagnostics.Services;

namespace PsyDiagnostics.ViewModels
{
    public class ResultViewModel : BaseViewModel
    {
        private MainViewModel _main;

        public string Risk { get; set; }

        public ICommand ToHistoryCommand { get; }

        public ResultViewModel(MainViewModel main, Dictionary<string, int> results)
        {
            _main = main;

            var ml = new MLPredictor();
            Risk = ml.Predict(results);

            // ✅ исправлено
            ToHistoryCommand = new RelayCommand(_ => _main.ShowHistory());
        }
    }
}