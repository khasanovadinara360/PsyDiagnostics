using System.Collections.Generic;

namespace PsyDiagnostics.Models
{
    public class Article
    {
        public string Number { get; set; }
        public string Title { get; set; }

        public List<string> Parts { get; set; }
        public List<string> Points { get; set; }

        public string Display => $"{Number} - {Title}";
    }
}