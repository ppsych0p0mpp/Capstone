using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Unsmoke.Helper;
using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.Views;

namespace Unsmoke.MVVM.ViewModel
{
    public class MyPlanVM : INotifyPropertyChanged
    {
        private readonly List<string> dailyTips = new List<string>
        {
            "Stay positive and take it one day at a time!",
            "Drink plenty of water to flush out toxins.",
            "Keep your hands busy with a hobby or fidget tools.",
            "Practice deep breathing techniques to manage cravings.",
            "Remember why you started quitting and stay motivated."
        };

        private string _tipOfTheDay;
        public string TipOfTheDay
        {
            get => _tipOfTheDay;
            set
            {
                if (_tipOfTheDay != value)
                {
                    _tipOfTheDay = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TipOfTheDay)));
                }
            }
        }
        public ObservableCollection<Education> Resources { get; set; }
        public ObservableCollection<Withdrawal> WithdrawalTips { get; set; }

        //Commands
        public ICommand OpenLinkCommand { get; set; }
        public ICommand GotoProfile { get; }

        public MyPlanVM()
        {
            UpdateTipOfTheDay();

            Resources = new ObservableCollection<Education>
            {
                new Education
                {
                    Title = "Health Effects of Smoking and the Benefits of Quitting",
                    Description = "Learn about the health risks of smoking from trusted health organizations.",
                    Url = "https://journalofethics.ama-assn.org/article/health-effects-smoking-and-benefits-quitting/2011-01",
                    Icon = "warnings.svg"
                },
                new Education
                {
                    Title = "WHO: Tobacco Facts",
                    Description = "Global facts and statistics about tobacco use.",
                    Url = "https://www.who.int/news-room/fact-sheets/detail/tobacco",
                    Icon = "hearts.svg"
                },
                new Education
                {
                    Title = "National Cancer Institute: Risks of Smoking",
                    Description = "Understand how smoking leads to various diseases including cancer.",
                    Url = "https://www.cancer.gov/about-cancer/causes-prevention/risk/tobacco",
                    Icon = "brains.svg"
                }
            };

            WithdrawalTips = new ObservableCollection<Withdrawal>
            {
                new Withdrawal
                {
                    Title = "Manage Cravings",
                    Tips = new ObservableCollection<string>
                    {
                        "Take 10 deep breaths when craving hits",
                        "Drink water to flush out toxins",
                        "Keep hands busy with fidget tools",
                        "Use the 4-7-8 breathing technique"
                    },
                    Icon = "thunder.svg"
                },
                new Withdrawal
                {
                    Title = "Replace Habits",
                    Tips = new ObservableCollection<string>
                    {
                        "Change your morning routine",
                        "Try herbal tea instead of cigarettes",
                        "Take a short walk during usual smoke breaks",
                        "Find a new hobby for your hands"
                    },
                    Icon = "coffee.svg"
                },
                new Withdrawal
                {
                    Title = "Stay Motivated",
                    Tips = new ObservableCollection<string>
                    {
                        "Remember your personal reasons for quitting",
                        "Calculate daily money saved",
                        "Share progress with supportive friends",
                        "Reward yourself for milestones reached"
                    },
                    Icon = "bulb.svg"
                },
                new Withdrawal
                {
                    Title = "Physical Symptoms",
                    Tips = new ObservableCollection<string>
                    {
                        "Headaches are normal for 2-4 weeks",
                        "Stay hydrated to reduce fatigue",
                        "Light exercise helps with irritability",
                        "Consult doctor if symptoms persist"
                    },
                    Icon = "security.svg"
                }
            };

            OpenLinkCommand = new Command<string>(async (url) => await Launcher.OpenAsync(url));
            GotoProfile = new AsyncRelayCommand(ToProfileAsync);
        }

        private async Task ToProfileAsync()
        {

            //Check if user is logged in
            var isLoggedIn = SessionManager.CurrentUser != null;

            if (!isLoggedIn)
            {
                // Show alert with OK and Cancel
                bool goToLogin = await Application.Current.MainPage.DisplayAlert(
                    "Login Required",
                    "Please login or register to access your profile.",
                    "Login",
                    "Cancel"); // returns true if "Login" pressed, false if "Cancel" pressed

                if (goToLogin)
                {
                    // Navigate to login page if user chooses "Login"
                    Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
                }

                return; // Exit method if user cancels
            }

            // If logged in, proceed to ProfilePage
            Application.Current.MainPage = App.Services.GetRequiredService<ProfilePage>();
        }
        private void UpdateTipOfTheDay()
        {
            int dayIndex = DateTime.Now.DayOfYear % dailyTips.Count;
            TipOfTheDay = dailyTips[dayIndex];
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
