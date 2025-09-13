using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unsmoke.MVVM.Models;


namespace Unsmoke.MVVM.ViewModel
{
    public partial class ProgressVM : ObservableObject
    {
        public ObservableCollection<Achievement> Achievements { get; set; }
        public ObservableCollection<Item> Items { get; set; }
        private Item _items = new Item();

        public ProgressVM()
        {
            Achievements = new ObservableCollection<Achievement>
                {
                    new Achievement { Title = "First Day", Description = "24 hours smoke-free", Icon = "firstday.png", IsUnlocked = true },
                    new Achievement { Title = "Money Saver", Description = "Saved ₱100", Icon = "moneysaver.png", IsUnlocked = true },
                    new Achievement { Title = "Health Hero", Description = "3 days clean", Icon = "healthhero.png", IsUnlocked = true },
                    new Achievement { Title = "Week Warrior", Description = "7 days smoke-free", Icon = "weekwarrior.png", IsUnlocked = false },
                    new Achievement { Title = "Strong Lungs", Description = "2 weeks clean", Icon = "lungs.png", IsUnlocked = false },
                    new Achievement { Title = "Champion", Description = "30 days milestone", Icon = "champion.png", IsUnlocked = false },
                    // Add the rest of your achievements here...
                };
            Items = new ObservableCollection<Item>();
        }

        //Craete the function here
        

    }
}
