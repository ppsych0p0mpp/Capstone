using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class ProgressVM : ObservableObject
    {
        public ObservableCollection<string> Achievements { get; set; }


        public ProgressVM()
        {

            Achievements = new ObservableCollection<string>
            {
                "After 1 day smoke-free",
                "Saved ₱100",
                "48 hours in",
                "3-day streak",
                "7 days smoke-free",
                "Saved ₱1,000",
                "2 weeks smoke-free",
                "21 days smoke-free",
                "Saved ₱5,000",
                "30 days no smoking",
                "45 days smoke-free",
                "60 days smoke-free",
                "90 days smoke-free (3 months)",
                "Saved ₱10,000",
                "120 days smoke-free (4 months)",
                "6 months smoke-free",
                "Saved ₱20,000",
                "9 months smoke-free",
                "1 year smoke-free",
                "Lifetime milestone – Freedom from smoking"
            };
        }

        //Function for the Achievements list
        //First condition if the Time Without Cigarette is greater than or equal to 1 day it will unlock the first achievement
        // Returns a list of unlocked achievements based on time without cigarette
       

    }
}
