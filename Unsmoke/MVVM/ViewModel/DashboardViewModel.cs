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
using Newtonsoft.Json.Linq;
using Unsmoke.Helper;
using Unsmoke.Service;


namespace Unsmoke.MVVM.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly FirestoreService _firestoreService;
        private string _assessmentDocId; // Store the documentId here after assessment is taken
        private bool hasCountedAvoidedToday = false;

        [ObservableProperty]
        private int addsmoke;

        [ObservableProperty]
        private DateTime lastSmokeTime = DateTime.Now;


        public DashboardData Data { get; set; }

        private Models.Assessment _assessment = new Models.Assessment();
        public ICommand AddCigarette { get;}
        public ICommand MinusCigarette { get; }

        private readonly IDispatcherTimer _timer;

        public DashboardViewModel()
        {
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            Data = new DashboardData
            {
                TimewithoutCig = TimeSpan.Zero,
                CigarettesSmokedToday = 0,
                CigarettedAvoided = 0,
                MoneySaved = 0,
                LifeTimeSaved = 0
            };


            AddCigarette = new RelayCommand(AddCigaretteAction);
            MinusCigarette = new RelayCommand(MinusCigaretteAction);
            Task.Run(LoadDashboardDataAsync);
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
        public string Days => ((int)(Data.TimewithoutCig.TotalDays % 30)).ToString("00");
        public string Hours => Data.TimewithoutCig.Hours.ToString("00");
        public string Minutes => Data.TimewithoutCig.Minutes.ToString("00");
        public string Seconds => Data.TimewithoutCig.Seconds.ToString("00");

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var userId = SessionManager.CurrentUser?.UserID;
                if (string.IsNullOrEmpty(userId)) return;

                var data = await _firestoreService.GetDocumentByIdAsync<DashboardData>("DashboardStats", userId);

                if (data != null)
                {
                    Data = data;
                    LastSmokeTime = data.QuitDate; // Load last smoke time
                    Data.TimewithoutCig = DateTime.UtcNow - LastSmokeTime; // Recalculate timer
                }
                else
                {
                    Data = new DashboardData
                    {
                        UserID = userId,
                        QuitDate = DateTime.UtcNow,
                        CigarettesSmokedToday = 0,
                        CigarettedAvoided = 0,
                        MoneySaved = 0,
                        LifeTimeSaved = 0
                    };

                    await SaveDashboardDataAsync();
                }

                RaiseElapsedChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard: {ex.Message}");
            }
        }


        private async Task SaveDashboardDataAsync()
        {
            try
            {
                var userId = SessionManager.CurrentUser?.UserID;
                if (string.IsNullOrEmpty(userId)) return;

                Data.UserID = userId;

                // Create or Update with same ID
                await _firestoreService.CreateOrUpdateDocumentAsync("DashboardStats", userId, Data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving dashboard: {ex.Message}");
            }
        }

        private void AddCigaretteAction()
        {
            Addsmoke = Data.CigarettesSmokedToday++;

            // Reset the QuitDate only when user smokes
            Data.QuitDate = DateTime.UtcNow;
            LastSmokeTime = Data.QuitDate;
            Data.TimewithoutCig = TimeSpan.Zero;
            hasCountedAvoidedToday = false;

            OnPropertyChanged(nameof(CigaretteToday));
            RaiseElapsedChanges();

            _ = SaveDashboardDataAsync();
        }
        private void MinusCigaretteAction()
        {
            if (Data.CigarettesSmokedToday > 0)
            {
                Data.CigarettesSmokedToday--;
                Addsmoke = Data.CigarettesSmokedToday;

                //Update cigarettes avoided
                OnPropertyChanged(nameof(CigaretteToday));
                _ = SaveDashboardDataAsync(); // Save progress
            }
        }

        //Add a funtion for time when click add smoke it will revert back the time to zero.
        private DateTime _lastSaveTime = DateTime.MinValue;

        private void UpdateElapsed()
        {
            Data.TimewithoutCig = DateTime.UtcNow - lastSmokeTime;

            // Calculate how many full days have passed
            int fullDays = (int)(Data.TimewithoutCig.TotalDays);

            if (fullDays > 0 && !hasCountedAvoidedToday)
            {
                // Cigarettes avoided = per day × number of full days
                int cigarettesAvoided = _assessment.CigarettesPerDay * fullDays;
                Data.CigarettedAvoided += cigarettesAvoided;

                // Money saved
                double moneyPerDay = _assessment.CigarettesPerDay * _assessment.CigaretteCost;
                Data.MoneySaved += moneyPerDay * fullDays;

                // Life saved (11 min per cigarette → convert to days)
                Data.LifeTimeSaved += (cigarettesAvoided * 11) / 1440.0;

                // Mark as counted so we don’t repeat on the same day
                hasCountedAvoidedToday = true;

                _ = SaveDashboardDataAsync();
            }

            // Auto-save every 60 seconds to Firestore
            if ((DateTime.UtcNow - _lastSaveTime).TotalSeconds >= 60)
            {
                _lastSaveTime = DateTime.UtcNow;
                _ = SaveDashboardDataAsync();
            }

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
