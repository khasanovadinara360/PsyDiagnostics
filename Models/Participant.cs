using System;
using System.Collections.Generic;
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

        private MaritalStatus _maritalStatus;
        public MaritalStatus MaritalStatus
        {
            get => _maritalStatus;
            set
            {
                _maritalStatus = value;
                OnPropertyChanged();
            }
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

        //public string Education { get; set; }
        //public string ProfessionBeforeConviction { get; set; }

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

        private EducationSurvey _educationLevel;
        public EducationSurvey EducationLevel
        {
            get => _educationLevel;
            set { _educationLevel = value; OnPropertyChanged(); }
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
                this[nameof(Profession)] == null &&
                this[nameof(ArticleNumber)] == null &&
                this[nameof(ArticlePart)] == null &&
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
                    // =========================
                    // ЛИЧНЫЕ
                    // =========================

                    case nameof(FullName):
                        if (string.IsNullOrWhiteSpace(FullName))
                            return "Введите ФИО";

                        var parts = FullName.Trim().Split(' ');

                        if (parts.Length < 3)
                            return "Минимум фамилия, имя и отчество";
                        break;

                    case nameof(Gender):
                        if (Gender == 0)
                            return "Выберите пол";
                        break;

                    case nameof(BirthDate):
                        if (BirthDate == DateTime.MinValue)
                            return "Укажите дату рождения";

                        if (BirthDate > DateTime.Now)
                            return "Дата рождения не может быть в будущем";

                        if (Age < 16)
                            return "Возраст должен быть не менее 16 лет";
                        break;

                    case nameof(Citizenship):
                        if (Citizenship == 0)
                            return "Выберите гражданство";
                        break;

                    case nameof(BirthPlace):
                        if (string.IsNullOrWhiteSpace(BirthPlace))
                            return "Введите место рождения";
                        break;

                    case nameof(Residence):
                        if (string.IsNullOrWhiteSpace(Residence))
                            return "Введите место проживания";
                        break;

                    case nameof(Nationality):
                        if (string.IsNullOrWhiteSpace(Nationality))
                            return "Выберите национальность";
                        break;

                    case nameof(FamilyUpbringing):
                        if (FamilyUpbringing == 0)
                            return "Укажите воспитание в семье";
                        break;

                    case nameof(MaritalStatus):
                        if (MaritalStatus == 0)
                            return "Выберите семейное положение";
                        break;

                    case nameof(HasCloseRelatives):
                        if (HasCloseRelatives == 0)
                            return "Укажите наличие близких родственников";
                        break;

                    case nameof(HasChildren):
                        if (HasChildren == 0)
                            return "Укажите наличие детей";
                        break;

                    case nameof(ChildrenCount):
                        if (HasChildren.ToString().Contains("Yes") &&
                            ChildrenCount <= 0)
                            return "Введите количество детей";
                        break;

                    case nameof(WillKeepContact):
                        if (WillKeepContact == 0)
                            return "Укажите поддержание связи с родственниками";
                        break;

                    case nameof(EducationLevel):
                        if (EducationLevel == 0)
                            return "Выберите образование";
                        break;

                    case nameof(HasProfession):
                        if (HasProfession == 0)
                            return "Укажите наличие профессии";
                        break;

                    case nameof(Profession):
                        if (HasProfession.ToString().Contains("Has") &&
                            string.IsNullOrWhiteSpace(Profession))
                            return "Введите профессию";
                        break;

                    case nameof(Religion):
                        if (Religion == 0)
                            return "Выберите вероисповедание";
                        break;

                    // =========================
                    // СОЦИАЛЬНЫЕ
                    // =========================

                    case nameof(ArmyService):
                        if (ArmyService == 0)
                            return "Укажите службу в армии";
                        break;

                    case nameof(CombatParticipation):
                        if (CombatParticipation == 0)
                            return "Укажите участие в боевых действиях";
                        break;

                    case nameof(SomaticDiseases):
                        if (SomaticDiseases == 0)
                            return "Укажите наличие соматических заболеваний";
                        break;

                    case nameof(Disability):
                        if (Disability == 0)
                            return "Укажите наличие инвалидности";
                        break;

                    case nameof(MentalDiseases):
                        if (MentalDiseases == 0)
                            return "Укажите наличие психических заболеваний";
                        break;

                    case nameof(PsychiatristRegistry):
                        if (PsychiatristRegistry == 0)
                            return "Укажите учет у психиатра";
                        break;

                    case nameof(Gambling):
                        if (Gambling == 0)
                            return "Укажите участие в азартных играх";
                        break;

                    case nameof(Obligations):
                        if (Obligations == 0)
                            return "Укажите наличие обязательств";
                        break;

                    case nameof(NarcologistRegistry):
                        if (NarcologistRegistry == 0)
                            return "Укажите учет у нарколога";
                        break;

                    case nameof(DrugUse):
                        if (DrugUse == 0)
                            return "Укажите употребление наркотических веществ";
                        break;

                    // =========================
                    // КРИМИНАЛЬНЫЕ
                    // =========================

                    case nameof(ArticleNumber):
                        if (string.IsNullOrWhiteSpace(ArticleNumber))
                            return "Введите статью";

                        if (!Regex.IsMatch(ArticleNumber, @"^\d+$"))
                            return "Только цифры";
                        break;

                    case nameof(ArticlePart):
                        if (string.IsNullOrWhiteSpace(ArticlePart))
                            return "Выберите часть статьи";
                        break;

                    case nameof(SentenceTerm):
                        if (SentenceTerm <= 0)
                            return "Срок должен быть больше 0";
                        break;

                    case nameof(CurrentFeelings):
                        if (CurrentFeelings == 0)
                            return "Укажите текущее эмоциональное состояние";
                        break;

                    case nameof(AttitudeToUIS):
                        if (AttitudeToUIS == 0)
                            return "Укажите отношение к пребыванию в УИС";
                        break;

                    case nameof(SuicideAttempts):
                        if (SuicideAttempts == 0)
                            return "Укажите наличие попыток суицида";
                        break;

                    case nameof(SelfHarmScars):
                        if (SelfHarmScars == 0)
                            return "Укажите наличие самоповреждений";
                        break;

                    case nameof(RelativesSuicide):
                        if (RelativesSuicide == 0)
                            return "Укажите случаи суицида у родственников";
                        break;

                    case nameof(CrimeType):
                        if (CrimeType == 0)
                            return "Выберите тип преступления";
                        break;

                    case nameof(Recidivism):
                        if (Recidivism == 0)
                            return "Выберите рецидив";
                        break;

                    case nameof(Unit):
                        if (string.IsNullOrWhiteSpace(Unit))
                            return "Введите отряд";
                        break;

                    case nameof(Category):
                        if (Category == 0)
                            return "Выберите категорию";
                        break;
                }

                return null;
            }
        }
        public List<string> GetErrors()
        {
            var errors = new List<string>();

            var props = new[]
            {
        // ЛИЧНЫЕ
        nameof(FullName),
        nameof(Gender),
        nameof(BirthDate),
        nameof(Citizenship),
        nameof(BirthPlace),
        nameof(Residence),
        nameof(Nationality),
        nameof(FamilyUpbringing),
        nameof(MaritalStatus),
        nameof(HasCloseRelatives),
        nameof(HasChildren),
        nameof(ChildrenCount),
        nameof(WillKeepContact),
        nameof(EducationLevel),
        nameof(HasProfession),
        nameof(Profession),
        nameof(Religion),

        // СОЦИАЛЬНЫЕ
        nameof(ArmyService),
        nameof(CombatParticipation),
        nameof(SomaticDiseases),
        nameof(Disability),
        nameof(MentalDiseases),
        nameof(PsychiatristRegistry),
        nameof(Gambling),
        nameof(Obligations),
        nameof(NarcologistRegistry),
        nameof(DrugUse),

        // КРИМИНАЛЬНЫЕ
        nameof(ArticleNumber),
        nameof(ArticlePart),
        nameof(SentenceTerm),
        nameof(CurrentFeelings),
        nameof(AttitudeToUIS),
        nameof(SuicideAttempts),
        nameof(SelfHarmScars),
        nameof(RelativesSuicide),
        nameof(CrimeType),
        nameof(Recidivism),
        nameof(Unit),
        nameof(Category)
    };

            foreach (var prop in props)
            {
                var error = this[prop];

                if (!string.IsNullOrEmpty(error))
                    errors.Add(error);
            }

            return errors;
        }

        public string Error => null;
    }
}