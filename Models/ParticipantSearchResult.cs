namespace PsyDiagnostics.Models
{
    public class ParticipantSearchResult
    {
        public string PrisonerId { get; set; }

        public string FullName { get; set; }

        public Citizenship Citizenship { get; set; }

        public int Age { get; set; }

        public string Residence { get; set; }

        public string ArticleNumber { get; set; }

        public string ArticlePart { get; set; }

        public string ArticlePoint { get; set; }

        public int SentenceTerm { get; set; }

        public string Unit { get; set; }

        public string Risk { get; set; }
    }
}