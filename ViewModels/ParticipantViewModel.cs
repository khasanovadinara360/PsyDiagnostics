using PsyDiagnostics.Helpers;
using PsyDiagnostics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

        public DateTime BirthDate
        {
            get => CurrentParticipant?.BirthDate ?? DateTime.Today;
            set
            {
                if (CurrentParticipant != null)
                    CurrentParticipant.BirthDate = value;

                OnPropertyChanged();
            }
        }

        public ICommand GoToTestCommand { get; }

        public Action<Participant> OnNavigateToTest { get; set; }

        public IReadOnlyList<Gender> Genders { get; } =
            GetEnumValues<Gender>();

        public IReadOnlyList<Citizenship> Citizenships { get; } =
            GetEnumValues<Citizenship>();

        public IReadOnlyList<FamilyUpbringing> FamilyUpbringings { get; } =
            GetEnumValues<FamilyUpbringing>();

        public IReadOnlyList<MaritalStatus> MaritalStatuses { get; } =
            GetEnumValues<MaritalStatus>();

        public IReadOnlyList<YesNo> YesNoValues { get; } =
            GetEnumValues<YesNo>();

        public IReadOnlyList<ChildrenPresence> ChildrenPresenceValues { get; } =
            GetEnumValues<ChildrenPresence>();

        public IReadOnlyList<EducationSurvey> EducationValues { get; } =
            GetEnumValues<EducationSurvey>();

        public IReadOnlyList<ProfessionPresence> ProfessionPresenceValues { get; } =
            GetEnumValues<ProfessionPresence>();

        public IReadOnlyList<Religion> ReligionValues { get; } =
            GetEnumValues<Religion>();

        public IReadOnlyList<ArmyService> ArmyServiceValues { get; } =
            GetEnumValues<ArmyService>();

        public IReadOnlyList<CombatParticipation> CombatParticipationValues { get; } =
            GetEnumValues<CombatParticipation>();

        public IReadOnlyList<SomaticDiseases> SomaticDiseasesValues { get; } =
            GetEnumValues<SomaticDiseases>();

        public IReadOnlyList<Disability> DisabilityValues { get; } =
            GetEnumValues<Disability>();

        public IReadOnlyList<MentalDiseases> MentalDiseasesValues { get; } =
            GetEnumValues<MentalDiseases>();

        public IReadOnlyList<PsychiatristRegistry> PsychiatristRegistryValues { get; } =
            GetEnumValues<PsychiatristRegistry>();

        public IReadOnlyList<Gambling> GamblingValues { get; } =
            GetEnumValues<Gambling>();

        public IReadOnlyList<Obligations> ObligationsValues { get; } =
            GetEnumValues<Obligations>();

        public IReadOnlyList<NarcologistRegistry> NarcologistRegistryValues { get; } =
            GetEnumValues<NarcologistRegistry>();

        public IReadOnlyList<DrugUse> DrugUseValues { get; } =
            GetEnumValues<DrugUse>();

        public IReadOnlyList<SuicideAttempts> SuicideAttemptsValues { get; } =
            GetEnumValues<SuicideAttempts>();

        public IReadOnlyList<SelfHarmScars> SelfHarmScarsValues { get; } =
            GetEnumValues<SelfHarmScars>();

        public IReadOnlyList<RelativesSuicide> RelativesSuicideValues { get; } =
            GetEnumValues<RelativesSuicide>();

        public IReadOnlyList<CurrentFeelings> CurrentFeelingsValues { get; } =
            GetEnumValues<CurrentFeelings>();

        public IReadOnlyList<AttitudeToUIS> AttitudeToUISValues { get; } =
            GetEnumValues<AttitudeToUIS>();

        public IReadOnlyList<CrimeType> CrimeTypes { get; } =
            GetEnumValues<CrimeType>();

        public IReadOnlyList<Recidivism> Recidivisms { get; } =
            GetEnumValues<Recidivism>();

        public IReadOnlyList<Category> Categories { get; } =
            GetEnumValues<Category>();

        public ParticipantViewModel()
        {
            GoToTestCommand =
                new RelayCommand(_ => GoToTest());
        }

        private void GoToTest()
        {
            if (CurrentParticipant == null)
            {
                MessageBox.Show(
                    "Сначала сохраните или выберите участника");

                return;
            }

            OnNavigateToTest?.Invoke(CurrentParticipant);
        }

        private static IReadOnlyList<T> GetEnumValues<T>()
            where T : Enum
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .ToList();
        }
    }
}