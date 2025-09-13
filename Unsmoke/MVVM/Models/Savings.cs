using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class Savings
    {
        public double totalSaved { get; set; }
        public double dailyGoal { get; set; }
        public double weeklyGoal { get; set; }
        public double monthlyGoal { get; set; }
        public double currentDaily { get; set; }
        public double currentWeekly { get; set; }
        public double currentMonthly { get; set; }
    }
}
