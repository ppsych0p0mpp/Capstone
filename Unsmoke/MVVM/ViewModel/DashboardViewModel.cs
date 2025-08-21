using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.Views;
using Microsoft.Maui.Dispatching;


namespace Unsmoke.MVVM.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int addsmoke;

        [ObservableProperty]
        private TimeSpan timewithoutCigarette = TimeSpan.Zero;

        [ObservableProperty]
        private DateTime lastSmokeTime = DateTime.Now;

        public DashboardData Data { get; set; }
        public ICommand AddCigarette { get;}
        public ICommand MinusCigarette { get; }

        private readonly IDispatcherTimer _timer;

        public DashboardViewModel()
        {
            Data = new DashboardData
            {
                CigarettesSmokedToday = 0
            };


            AddCigarette = new RelayCommand(AddCigaretteAction);
            MinusCigarette = new RelayCommand(MinusCigaretteAction);

            // MAUI timer
            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);   // update every second
            _timer.Tick += (_, __) => UpdateElapsed();
            _timer.Start();
        }

        public int CigaretteToday => Data.CigarettesSmokedToday;

        // Nicely formatted composite string if you want one label
        public string ElapsedFormatted => $"{Days}:{Hours}:{Minutes}:{Seconds}";

        // Separate parts for binding to individual labels
        public string Days => ((int)(timewithoutCigarette.TotalDays % 30)).ToString("00");
        public string Hours => timewithoutCigarette.Hours.ToString("00");
        public string Minutes => timewithoutCigarette.Minutes.ToString("00");
        public string Seconds => timewithoutCigarette.Seconds.ToString("00");


        private void AddCigaretteAction()
        {
            addsmoke = Data.CigarettesSmokedToday++;

            //reset Timer
            lastSmokeTime = DateTime.Now;
            timewithoutCigarette = TimeSpan.Zero; // Reset the time without cigarette

            OnPropertyChanged(nameof(CigaretteToday));
            RaiseElapsedChanges();
        }
        private void MinusCigaretteAction()
        {
            if (Data.CigarettesSmokedToday > 0)
            {
                Data.CigarettesSmokedToday--;
                addsmoke = Data.CigarettesSmokedToday;
                OnPropertyChanged(nameof(CigaretteToday));
            }
        }

        //Add a funtion for time when click add smoke it will revert back the time to zero.

        private void UpdateElapsed()
        {
            timewithoutCigarette = DateTime.Now - lastSmokeTime;
            RaiseElapsedChanges();
        }
        private void RaiseElapsedChanges()
        {
            OnPropertyChanged(nameof(Days));
            OnPropertyChanged(nameof(Hours));
            OnPropertyChanged(nameof(Minutes));
            OnPropertyChanged(nameof(Seconds));
            OnPropertyChanged(nameof(ElapsedFormatted));
        }
    }
}
