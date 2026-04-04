using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsyDiagnostics.Models.Tests
{
    public class TestScale
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<ResultRecord> ResultRecord { get; set; }
    }
}