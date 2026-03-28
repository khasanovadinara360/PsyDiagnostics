using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PsyDiagnostics.Models
{
    public class Participant : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string PrisonerId { get; set; }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private DateTime _birthDate;
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Age));
            }
        }

        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;

                if (BirthDate > today.AddYears(-age))
                    age--;

                return age;
            }
        }

        public string BirthPlace { get; set; }
        public Citizenship Citizenship { get; set; }

        public EducationLevel EducationLevel { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public bool HasChildren { get; set; }

        public string ProfessionBeforeConviction { get; set; }

        // 🔥 СТАТЬЯ
        public string ArticleNumber { get; set; }
        public string ArticlePart { get; set; }
        public string ArticlePoint { get; set; }

        public string CriminalArticle =>
            $"{ArticleNumber}" +
            (string.IsNullOrWhiteSpace(ArticlePart) ? "" : $" ч.{ArticlePart}") +
            (string.IsNullOrWhiteSpace(ArticlePoint) ? "" : $" п.«{ArticlePoint}»") +
            " УК РФ";

        // 🔥 СРОК
        private string _sentenceTerm;
        public string SentenceTerm
        {
            get => _sentenceTerm;
            set
            {
                _sentenceTerm = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SentenceTermDisplay));
            }
        }

        public string SentenceTermDisplay
        {
            get
            {
                if (!int.TryParse(SentenceTerm, out int years))
                    return "";

                if (years == 1)
                    return "1 год";

                if (years >= 2 && years <= 4)
                    return $"{years} года";

                return $"{years} лет";
            }
        }

        public CrimeType CrimeType { get; set; }

        // 🔥🔥🔥 РЕЦИДИВ (СВЯЗАН С СУДИМОСТЯМИ)
        private Recidivism _recidivism;
        public Recidivism Recidivism
        {
            get => _recidivism;
            set
            {
                _recidivism = value;

                // если нет рецидива → 0 судимостей
                if (_recidivism == Recidivism.Нет)
                    _previousConvictions = 0;

                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviousConvictions));
            }
        }

        // 🔥🔥🔥 СУДИМОСТИ (ДЛЯ ИИ)
        private int _previousConvictions;
        public int PreviousConvictions
        {
            get => _previousConvictions;
            set
            {
                _previousConvictions = value;

                // если есть судимости → рецидив = Да
                if (_previousConvictions > 0)
                    _recidivism = Recidivism.Да;
                else
                    _recidivism = Recidivism.Нет;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Recidivism));
            }
        }

        public string Unit { get; set; }
        public Category Category { get; set; }

        // 🔥 ВАЛИДАЦИЯ
        public bool IsValid()
        {
            return
                this[nameof(FullName)] == null &&
                this[nameof(BirthDate)] == null &&
                this[nameof(BirthPlace)] == null &&
                this[nameof(ProfessionBeforeConviction)] == null &&
                this[nameof(ArticleNumber)] == null &&
                this[nameof(SentenceTerm)] == null &&
                this[nameof(Unit)] == null &&
                this[nameof(PreviousConvictions)] == null;
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(FullName):
                        if (string.IsNullOrWhiteSpace(FullName))
                            return "Введите ФИО";

                        var parts = FullName.Split(' ');
                        if (parts.Length < 2)
                            return "Минимум фамилия и имя";
                        break;

                    case nameof(BirthDate):
                        if (BirthDate > DateTime.Now)
                            return "Дата в будущем";
                        break;

                    case nameof(BirthPlace):
                        if (string.IsNullOrWhiteSpace(BirthPlace))
                            return "Введите место рождения";
                        break;

                    case nameof(ProfessionBeforeConviction):
                        if (string.IsNullOrWhiteSpace(ProfessionBeforeConviction))
                            return "Введите профессию";
                        break;

                    case nameof(ArticleNumber):
                        if (string.IsNullOrWhiteSpace(ArticleNumber))
                            return "Введите статью";

                        if (!Regex.IsMatch(ArticleNumber, @"^\d+$"))
                            return "Только цифры";
                        break;

                    case nameof(SentenceTerm):
                        if (string.IsNullOrWhiteSpace(SentenceTerm))
                            return "Введите срок";

                        if (!Regex.IsMatch(SentenceTerm, @"^\d+$"))
                            return "Только цифры";

                        if (int.Parse(SentenceTerm) <= 0)
                            return "Срок должен быть больше 0";
                        break;

                    case nameof(Unit):
                        if (!Regex.IsMatch(Unit ?? "", @"^\d+$"))
                            return "Только цифры";
                        break;

                    case nameof(PreviousConvictions):
                        if (PreviousConvictions < 0)
                            return "Не может быть меньше 0";
                        break;
                }

                return null;
            }
        }

        public string Error => null;
    }
}