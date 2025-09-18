using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.Helper;
using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.Views;
using Unsmoke.Service;


namespace Unsmoke.MVVM.ViewModel
{
    public partial class ProgressVM : ObservableObject
    {
        private readonly FirestoreService _firestoreService;

        public DashboardData Data { get; set; }

        [ObservableProperty]
        private Savings _savings = new Savings();

        [ObservableProperty]
        private double moneySaved;

        [ObservableProperty]
        private double dailyGoalProgress;

        [ObservableProperty]
        private double weeklyGoalProgress;

        [ObservableProperty]
        private double monthlyGoalProgress;

        public ObservableCollection<Achievement> Achievements { get; set; }

        public ObservableCollection<Item> Items { get; set; } = new();  // Store added items


        [ObservableProperty]
        private bool showprog = false;

        [ObservableProperty]
        private bool itemcomp = true;


        [ObservableProperty]
        private string itemName;   // Bound to Entry NAME

        [ObservableProperty]
        private string itemPrice;  // Bound to Entry PRICE

        [ObservableProperty]
        private string image = "add.svg";


        //Commands
        public ICommand ShowProgress { get; }
        public ICommand ItemComp { get; }
        public ICommand AddItemCommand { get; }
        public ICommand PickImageCommand { get; }
        public ICommand GotoProfile { get; }

        //Constructor
        public ProgressVM()
        {
            Achievements = new ObservableCollection<Achievement>
                {
                    new Achievement { Title = "First Day", Description = "24 hours smoke-free", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Money Saver", Description = "Saved ₱100", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Health Hero", Description = "3 days clean", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Week Warrior", Description = "7 days smoke-free", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Strong Lungs", Description = "2 weeks clean", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Champion", Description = "30 days milestone", Icon = "unlock", IsUnlocked = false },
                    // Add the rest of your achievements here...
                };

            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            ShowProgress = new RelayCommand(Progress);
            ItemComp = new RelayCommand(ShowItemComparison);
            AddItemCommand = new AsyncRelayCommand(AddItemAsync);
            PickImageCommand = new AsyncRelayCommand(PickImageAsync);
            GotoProfile = new AsyncRelayCommand(ToProfileAsync);

            Task.Run(LoadDashboardDataAsync);
        }

        private int index = 1;

        // Show the Progress Section
        public void Progress()
        {
            Showprog = true;   // Show progress section
            Itemcomp = false;  // Hide item comparison section (optional)
        }
        public void ShowItemComparison()
        {
            Showprog = false;
            Itemcomp = true;
        }
        private async Task PickImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an Image"
                });

                if (result != null)
                {
                    Image = result.FullPath; // Set new image from device
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Image selection failed: {ex.Message}", "OK");
            }
        }

        // ADD ITEM FUNCTION
        private async Task AddItemAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ItemName) || string.IsNullOrWhiteSpace(ItemPrice))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill out both fields", "OK");
                return;
            }

            if (!double.TryParse(ItemPrice, out double price))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Price must be a valid number", "OK");
                return;
            }

            // Create item object
            var newItem = new Item
            {
                ItemName = ItemName,
                ItemPrice = price,
                Image = image // Placeholder, implement image handling as needed
            };

            // Save to Firestore
            await _firestoreService.AddDocumentAsync("ItemComparison", newItem);

            // Add to local collection for UI
            Items.Add(newItem);

            await Application.Current.MainPage.DisplayAlert("Success", "Item Added!", "OK");
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

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Get the dashboard data for the current user
                var docId = SessionManager.CurrentUser?.UserID.ToString();
                if (string.IsNullOrEmpty(docId)) return;

                var dashboard = await _firestoreService.GetDocumentByIdAsync<DashboardData>("DashboardStats", docId);
                if (dashboard == null) return;

                Data = dashboard;
                MoneySaved = dashboard.MoneySaved;

                // Update goal progress
                DailyGoalProgress = MoneySaved / 50;    
                WeeklyGoalProgress = MoneySaved / 350;  
                MonthlyGoalProgress = MoneySaved / 1500;

                // Check and unlock achievements
                CheckAchievements();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        private async void CheckAchievements()
        {
            foreach (var achievement in Achievements)
            {
                bool wasUnlocked = achievement.IsUnlocked; // Track old state

                switch (achievement.Title)
                {
                    case "First Day":
                        if (Data.TimewithoutCig >= TimeSpan.FromDays(1))
                            achievement.IsUnlocked = true;
                        break;

                    case "Money Saver":
                        if (MoneySaved >= 100)
                            achievement.IsUnlocked = true;
                        break;

                    case "Health Hero":
                        if (Data.TimewithoutCig >= TimeSpan.FromDays(3))
                            achievement.IsUnlocked = true;
                        break;

                    case "Week Warrior":
                        if (Data.TimewithoutCig >= TimeSpan.FromDays(7))
                            achievement.IsUnlocked = true;
                        break;

                    case "Strong Lungs":
                        if (Data.TimewithoutCig >= TimeSpan.FromDays(14))
                            achievement.IsUnlocked = true;
                        break;

                    case "Champion":
                        if (Data.TimewithoutCig >= TimeSpan.FromDays(30))
                            achievement.IsUnlocked = true;
                        break;
                }

                // Show alert only when it’s newly unlocked
                if (!wasUnlocked && achievement.IsUnlocked)
                {
                   await Application.Current.MainPage.DisplayAlert(
                        "Achievement Unlocked!",
                        $"{achievement.Title} - {achievement.Description}",
                        "OK");
                }
            }
        }
    }
}
