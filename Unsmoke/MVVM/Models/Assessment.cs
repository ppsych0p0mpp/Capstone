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
        public int YearsOfSmoking { get; set; }
        public string YearMonth { get; set; }
        public double CigaretteCost { get; set; }
        public double TotalCost { get; set; }
        public double MoneySaved { get; set; }
        public string SmokingCategory { get; set; }
        public int CigarettesPerDay { get; set; }




    }
}
