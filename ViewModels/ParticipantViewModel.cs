using System;
using System.Collections.Generic;
using System.Linq;
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

        // Поиск теперь живёт в MainViewModel, поэтому здесь он не нужен.
        // Оставляем только то, что относится к анкете и переходу к тестам.

        public ICommand GoToTestCommand { get; }
        public Action<Participant> OnNavigateToTest;

        // Коллекции значений enum для ComboBox
        public IReadOnlyList<Gender> Genders { get; } =
            Enum.GetValues(typeof(Gender)).Cast<Gender>().ToList();

        public IReadOnlyList<FamilyUpbringing> FamilyUpbringings { get; } =
            Enum.GetValues(typeof(FamilyUpbringing)).Cast<FamilyUpbringing>().ToList();

        public IReadOnlyList<MaritalStatus> MaritalStatuses { get; } =
            Enum.GetValues(typeof(MaritalStatus)).Cast<MaritalStatus>().ToList();

        public IReadOnlyList<YesNo> YesNoValues { get; } =
            Enum.GetValues(typeof(YesNo)).Cast<YesNo>().ToList();

        public IReadOnlyList<ChildrenPresence> ChildrenPresenceValues { get; } =
            Enum.GetValues(typeof(ChildrenPresence)).Cast<ChildrenPresence>().ToList();

        public IReadOnlyList<EducationSurvey> EducationValues { get; } =
            Enum.GetValues(typeof(EducationSurvey)).Cast<EducationSurvey>().ToList();

        public IReadOnlyList<ProfessionPresence> ProfessionPresenceValues { get; } =
            Enum.GetValues(typeof(ProfessionPresence)).Cast<ProfessionPresence>().ToList();

        public IReadOnlyList<Religion> ReligionValues { get; } =
            Enum.GetValues(typeof(Religion)).Cast<Religion>().ToList();

        public IReadOnlyList<ArmyService> ArmyServiceValues { get; } =
            Enum.GetValues(typeof(ArmyService)).Cast<ArmyService>().ToList();

        public IReadOnlyList<CombatParticipation> CombatParticipationValues { get; } =
            Enum.GetValues(typeof(CombatParticipation)).Cast<CombatParticipation>().ToList();

        public IReadOnlyList<SomaticDiseases> SomaticDiseasesValues { get; } =
            Enum.GetValues(typeof(SomaticDiseases)).Cast<SomaticDiseases>().ToList();

        public IReadOnlyList<Disability> DisabilityValues { get; } =
            Enum.GetValues(typeof(Disability)).Cast<Disability>().ToList();

        public IReadOnlyList<MentalDiseases> MentalDiseasesValues { get; } =
            Enum.GetValues(typeof(MentalDiseases)).Cast<MentalDiseases>().ToList();

        public IReadOnlyList<PsychiatristRegistry> PsychiatristRegistryValues { get; } =
            Enum.GetValues(typeof(PsychiatristRegistry)).Cast<PsychiatristRegistry>().ToList();

        public IReadOnlyList<Gambling> GamblingValues { get; } =
            Enum.GetValues(typeof(Gambling)).Cast<Gambling>().ToList();

        public IReadOnlyList<Obligations> ObligationsValues { get; } =
            Enum.GetValues(typeof(Obligations)).Cast<Obligations>().ToList();

        public IReadOnlyList<NarcologistRegistry> NarcologistRegistryValues { get; } =
            Enum.GetValues(typeof(NarcologistRegistry)).Cast<NarcologistRegistry>().ToList();

        public IReadOnlyList<DrugUse> DrugUseValues { get; } =
            Enum.GetValues(typeof(DrugUse)).Cast<DrugUse>().ToList();

        public IReadOnlyList<SuicideAttempts> SuicideAttemptsValues { get; } =
            Enum.GetValues(typeof(SuicideAttempts)).Cast<SuicideAttempts>().ToList();

        public IReadOnlyList<SelfHarmScars> SelfHarmScarsValues { get; } =
            Enum.GetValues(typeof(SelfHarmScars)).Cast<SelfHarmScars>().ToList();

        public IReadOnlyList<RelativesSuicide> RelativesSuicideValues { get; } =
            Enum.GetValues(typeof(RelativesSuicide)).Cast<RelativesSuicide>().ToList();

        public IReadOnlyList<CurrentFeelings> CurrentFeelingsValues { get; } =
            Enum.GetValues(typeof(CurrentFeelings)).Cast<CurrentFeelings>().ToList();

        public IReadOnlyList<AttitudeToUIS> AttitudeToUISValues { get; } =
            Enum.GetValues(typeof(AttitudeToUIS)).Cast<AttitudeToUIS>().ToList();

        public IReadOnlyList<CrimeType> CrimeTypes { get; } =
            Enum.GetValues(typeof(CrimeType)).Cast<CrimeType>().ToList();

        public IReadOnlyList<Recidivism> Recidivisms { get; } =
            Enum.GetValues(typeof(Recidivism)).Cast<Recidivism>().ToList();

        public IReadOnlyList<Category> Categories { get; } =
            Enum.GetValues(typeof(Category)).Cast<Category>().ToList();

        public ParticipantViewModel()
        {
            GoToTestCommand = new RelayCommand(_ => GoToTest());
        }

        private void GoToTest()
        {
            if (CurrentParticipant == null)
            {
                MessageBox.Show("Сначала сохраните или выберите участника");
                return;
            }

            OnNavigateToTest?.Invoke(CurrentParticipant);
        }
    }
}