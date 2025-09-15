using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class Assessment
    {
        public int AssessmentID { get; set; }
        public string UserID { get; set; }
        public DateTime DateTaken { get; set; }
        public string Gender { get; set; }
        public int DurationOfSmoking { get; set; }
        public string YearMonth { get; set; }
        public double CigaretteCost { get; set; }
        public int CigarettesPerDay { get; set; }
        public string ConfidenceLevel { get; set; }

    }
}
