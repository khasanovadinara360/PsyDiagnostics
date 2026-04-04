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

        private Gender _gender;
        public Gender Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
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
                if (BirthDate > today.AddYears(-age)) age--;
                return age < 0 ? 0 : age;
            }
        }

        public string BirthPlace { get; set; }
        public string Nationality { get; set; }
        public string Residence { get; set; }

        public Citizenship Citizenship { get; set; }
        public EducationLevel EducationLevel { get; set; }

        private MaritalStatus _maritalStatus;
        public MaritalStatus MaritalStatus
        {
            get => _maritalStatus;
            set { _maritalStatus = value; OnPropertyChanged(); }
        }

        // Есть ли дети – enum, а не bool
        private ChildrenPresence _hasChildren;
        public ChildrenPresence HasChildren
        {
            get => _hasChildren;
            set { _hasChildren = value; OnPropertyChanged(); }
        }

        public int ChildrenCount { get; set; }

        private FamilyUpbringing _familyUpbringing;
        public FamilyUpbringing FamilyUpbringing
        {
            get => _familyUpbringing;
            set { _familyUpbringing = value; OnPropertyChanged(); }
        }

        private YesNo _hasCloseRelatives;
        public YesNo HasCloseRelatives
        {
            get => _hasCloseRelatives;
            set { _hasCloseRelatives = value; OnPropertyChanged(); }
        }

        private YesNo _willKeepContact;
        public YesNo WillKeepContact
        {
            get => _willKeepContact;
            set { _willKeepContact = value; OnPropertyChanged(); }
        }

        public string Education { get; set; }
        public string ProfessionBeforeConviction { get; set; }

        private ProfessionPresence _hasProfession;
        public ProfessionPresence HasProfession
        {
            get => _hasProfession;
            set { _hasProfession = value; OnPropertyChanged(); }
        }

        public string Profession { get; set; }

        private ArmyService _armyService;
        public ArmyService ArmyService
        {
            get => _armyService;
            set { _armyService = value; OnPropertyChanged(); }
        }

        public string ArmyBranch { get; set; }

        private CombatParticipation _combatParticipation;
        public CombatParticipation CombatParticipation
        {
            get => _combatParticipation;
            set { _combatParticipation = value; OnPropertyChanged(); }
        }

        private SomaticDiseases _somaticDiseases;
        public SomaticDiseases SomaticDiseases
        {
            get => _somaticDiseases;
            set { _somaticDiseases = value; OnPropertyChanged(); }
        }

        private Disability _disability;
        public Disability Disability
        {
            get => _disability;
            set { _disability = value; OnPropertyChanged(); }
        }

        private MentalDiseases _mentalDiseases;
        public MentalDiseases MentalDiseases
        {
            get => _mentalDiseases;
            set { _mentalDiseases = value; OnPropertyChanged(); }
        }

        private PsychiatristRegistry _psychiatristRegistry;
        public PsychiatristRegistry PsychiatristRegistry
        {
            get => _psychiatristRegistry;
            set { _psychiatristRegistry = value; OnPropertyChanged(); }
        }

        private Gambling _gambling;
        public Gambling Gambling
        {
            get => _gambling;
            set { _gambling = value; OnPropertyChanged(); }
        }

        private SuicideAttempts _suicideAttempts;
        public SuicideAttempts SuicideAttempts
        {
            get => _suicideAttempts;
            set { _suicideAttempts = value; OnPropertyChanged(); }
        }

        private SelfHarmScars _selfHarmScars;
        public SelfHarmScars SelfHarmScars
        {
            get => _selfHarmScars;
            set { _selfHarmScars = value; OnPropertyChanged(); }
        }

        private RelativesSuicide _relativesSuicide;
        public RelativesSuicide RelativesSuicide
        {
            get => _relativesSuicide;
            set { _relativesSuicide = value; OnPropertyChanged(); }
        }

        private CurrentFeelings _currentFeelings;
        public CurrentFeelings CurrentFeelings
        {
            get => _currentFeelings;
            set { _currentFeelings = value; OnPropertyChanged(); }
        }

        private AttitudeToUIS _attitudeToUIS;
        public AttitudeToUIS AttitudeToUIS
        {
            get => _attitudeToUIS;
            set { _attitudeToUIS = value; OnPropertyChanged(); }
        }

        private Obligations _obligations;
        public Obligations Obligations
        {
            get => _obligations;
            set { _obligations = value; OnPropertyChanged(); }
        }

        private Religion _religion;
        public Religion Religion
        {
            get => _religion;
            set { _religion = value; OnPropertyChanged(); }
        }

        private NarcologistRegistry _narcologistRegistry;
        public NarcologistRegistry NarcologistRegistry
        {
            get => _narcologistRegistry;
            set { _narcologistRegistry = value; OnPropertyChanged(); }
        }

        private DrugUse _drugUse;
        public DrugUse DrugUse
        {
            get => _drugUse;
            set { _drugUse = value; OnPropertyChanged(); }
        }

        public string ArticleNumber { get; set; }
        public string ArticlePart { get; set; }
        public string ArticlePoint { get; set; }

        public string CriminalArticle =>
            $"{ArticleNumber}" +
            (string.IsNullOrWhiteSpace(ArticlePart) ? "" : $" ч.{ArticlePart}") +
            (string.IsNullOrWhiteSpace(ArticlePoint) ? "" : $" п.«{ArticlePoint}»") +
            " УК РФ";

        // Срок как число лет
        private int _sentenceTerm;
        public int SentenceTerm
        {
            get => _sentenceTerm;
            set { _sentenceTerm = value; OnPropertyChanged(); OnPropertyChanged(nameof(SentenceTermDisplay)); }
        }

        public string SentenceTermDisplay
        {
            get
            {
                var years = SentenceTerm;
                if (years <= 0) return "";

                if (years == 1) return "1 год";
                if (years >= 2 && years <= 4) return $"{years} года";
                return $"{years} лет";
            }
        }

        public CrimeType CrimeType { get; set; }

        private Recidivism _recidivism;
        public Recidivism Recidivism
        {
            get => _recidivism;
            set
            {
                _recidivism = value;
                if (_recidivism == Recidivism.Нет)
                    _previousConvictions = 0;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviousConvictions));
            }
        }

        private int _previousConvictions;
        public int PreviousConvictions
        {
            get => _previousConvictions;
            set
            {
                _previousConvictions = value;
                _recidivism = _previousConvictions > 0 ? Recidivism.Да : Recidivism.Нет;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Recidivism));
            }
        }

        public string Unit { get; set; }
        public Category Category { get; set; }

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

                    case nameof(Nationality):
                        // можно без ошибок, но чтобы не было пустого имени
                        if (string.IsNullOrWhiteSpace(Nationality))
                            return "Введите национальность";
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
                        if (SentenceTerm <= 0)
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