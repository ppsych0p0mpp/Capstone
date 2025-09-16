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


        public Models.Assessment assessment = new Models.Assessment();

        [ObservableProperty]
        private int addsmoke;

        [ObservableProperty]
        private DateTime lastSmokeTime = DateTime.Now;


        public DashboardData Data { get; set; }

        private Models.Assessment _assessment = new Models.Assessment();
        public ICommand AddCigarette { get;}
        public ICommand MinusCigarette { get; }
        public ICommand LoadDashboardDataCommand { get; }

        private readonly IDispatcherTimer _timer;

        public DashboardViewModel()
        {
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            Data = new DashboardData
            {
                CigarettesSmokedToday = 0,
                CigarettedAvoided = 0,
                MoneySaved = 0,
                LifeTimeSaved = 0
            };


            AddCigarette = new RelayCommand(AddCigaretteAction);
            MinusCigarette = new RelayCommand(MinusCigaretteAction);
            //LoadDashboardDataCommand = new AsyncRelayCommand(LoadDashboardDataAsync);

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

        //private async Task LoadDashboardDataAsync()
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(_assessmentDocId)) return;

        //        var data = await _firestoreService.GetDocumentsAsync<DashboardData>("DashboardStats", _assessmentDocId);
        //        if (data != null)
        //        {
        //            Data = data;
        //            lastSmokeTime = data.QuitDate; // or saved LastSmokeTime
        //            RaiseElapsedChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error loading dashboard: {ex.Message}");
        //    }
        //}
        private async Task SaveDashboardDataAsync()
        {
            try
            {
                var userId = SessionManager.CurrentUser?.UserID ?? 0;
                if (userId == 0) return;

                var saveData = new
                {
                    CigarettesSmokedToday = Data.CigarettesSmokedToday,
                    CigarettedAvoided = Data.CigarettedAvoided,
                    MoneySaved = Data.MoneySaved,
                    LifeTimeSaved = Data.LifeTimeSaved,
                    LastSmokeTime = lastSmokeTime
                };

                await _firestoreService.UpdateDocumentAsync("DashboardStats", userId.ToString(), saveData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving dashboard: {ex.Message}");
            }
        }
        private void AddCigaretteAction()
        {
            addsmoke = Data.CigarettesSmokedToday++;
            //reset Timer
            lastSmokeTime = DateTime.Now;
            Data.TimewithoutCig = TimeSpan.Zero; // Reset the time without cigarette
            hasCountedAvoidedToday = false;

            OnPropertyChanged(nameof(CigaretteToday));
            RaiseElapsedChanges();

            _ = SaveDashboardDataAsync();// Save progress
        }
        private void MinusCigaretteAction()
        {
            if (Data.CigarettesSmokedToday > 0)
            {
                Data.CigarettesSmokedToday--;
                addsmoke = Data.CigarettesSmokedToday;

                //Update cigarettes avoided
                OnPropertyChanged(nameof(CigaretteToday));
                _ = SaveDashboardDataAsync(); // Save progress
            }
        }

        //Add a funtion for time when click add smoke it will revert back the time to zero.
        private void UpdateElapsed()
        {

            Data.TimewithoutCig = DateTime.Now - lastSmokeTime;
            // When 24 hours passed without smoking
            if (Data.TimewithoutCig.TotalHours >= 24 && !hasCountedAvoidedToday)
            {
                Data.CigarettedAvoided = assessment.CigarettesPerDay; // Increment by daily average
                hasCountedAvoidedToday = true;

                // Example: Money saved logic
                Data.MoneySaved = assessment.CigaretteCost;// Increment by daily cost

                // Example: Life time saved (e.g., 11 min per cigarette)
                Data.LifeTimeSaved += (Data.CigarettesSmokedToday * 11) / 1440.0; // Days saved

                _ = SaveDashboardDataAsync(); // Save progress
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

        //Add function for Cigarette avoided increment when time without cigarette reach 24 hour



    }
}
