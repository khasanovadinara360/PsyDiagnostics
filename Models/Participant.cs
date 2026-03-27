using System;

namespace PsyDiagnostics.Models
{
    public class Participant
    {
        // для БД
        public string Id { get; set; }
        public string Name { get; set; }

        // для UI (синоним)
        public string FullName
        {
            get => Name;
            set => Name = value;
        }

        public DateTime? BirthDate { get; set; }

        public int Age =>
            BirthDate.HasValue
                ? DateTime.Now.Year - BirthDate.Value.Year
                : 0;

        public string BirthPlace { get; set; }
        public string Citizenship { get; set; }

        public string EducationLevel { get; set; }
        public string MaritalStatus { get; set; }
        public bool HasChildren { get; set; }
        public string ProfessionBeforeConviction { get; set; }

        public string CriminalArticle { get; set; }
        public string ArticlePart { get; set; }
        public string ArticlePoint { get; set; }

        public int SentenceTerm { get; set; }
        public string SentenceTermDisplay => $"{SentenceTerm} лет";

        public string CrimeType { get; set; }
        public string Recidivism { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
    }
}