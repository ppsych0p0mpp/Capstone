using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class DashboardData
    {
        public int UserID { get; set; }//Foreign Key

        //Smoking journey info
        public string SmokingLevel { get; set; }
        public string AchiementBadge { get; set; }

        //Time without smoking
        public DateTime QuitDate { get; set; }

        //Statistics
        public int CigarettedAvoided { get; set; }
        public double MoneySaved { get; set; }
        public double LifeTimeSaved { get; set; }

        //Today's data
        public int CigarettesSmokedToday { get; set; }

        public DashboardData()
        {
            QuitDate = DateTime.UtcNow;
        }
    }
}
