using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
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
    public partial class ProfileVM : ObservableObject
    {
        private readonly FirestoreService __firestoreService;
        private readonly CultureInfo pesoCulture = new CultureInfo("en-PH");

        public ICommand GotoDash { get; }
        public ICommand Logout { get; }

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string assessmentDate;

        [ObservableProperty]
        private Users _user = new Users();

        [ObservableProperty]
        private  Savings _savings = new Savings();

        [ObservableProperty]
        private Models.Assessment _assessment = new Models.Assessment();

        private string _summaryMessage;
        public string SummaryMessage
        {
            get => _summaryMessage;
            set
            {
                _summaryMessage = value;
                OnPropertyChanged();
            }
        }

        public ProfileVM()
        {
            __firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            GotoDash = new RelayCommand(Backto);
            Logout = new AsyncRelayCommand(LogoutUserAsync);
            Task.Run(DisplayAssessmentAsync);
        }

        private void Backto()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            return;
        }

        //Display Assessment Summary
        // Display Assessment Summary
        private async Task DisplayAssessmentAsync()
        {
            if (SessionManager.CurrentUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please login first to view your assessment.", "OK");
                return;
            }

            // Get logged-in user
            var userId = SessionManager.CurrentUser.UserID;
            FullName = SessionManager.CurrentUser.FullName;

            // Fetch assessment by userId field instead of document ID
            var assessments = await __firestoreService.QueryDocumentsAsync<Models.Assessment>(
                "assessments",
                "UserID", userId // field name in Firestore
            );

            _assessment = assessments.FirstOrDefault();

            if (_assessment == null)
            {
                await Application.Current.MainPage.DisplayAlert("No Data", "No assessment found for this user.", "OK");
                return;
            }

            // Set assessment date
            AssessmentDate = _assessment.DateTaken.ToString("MMMM dd, yyyy");

            // Calculate total days
            double daysSmoked = _assessment.YearMonth == "Years"
                ? _assessment.DurationOfSmoking * 365
                : _assessment.DurationOfSmoking * 30;

            // Daily cost
            double dailyCost = _assessment.CigarettesPerDay * _assessment.CigaretteCost;

            // Total money spent
            double moneySpent = dailyCost * daysSmoked;

            // Savings calculations
            double dailySavings = dailyCost;
            double weeklySavings = dailyCost * 7;
            double monthlySavings = dailyCost * 30;

            // Store in Savings model
            _savings.totalSaved = moneySpent;
            _savings.Daily = dailySavings;
            _savings.Weekly = weeklySavings;
            _savings.Monthly = monthlySavings;

            // Build the summary message
            SummaryMessage = $"Gender: {_assessment.Gender}\n" +
                             $"Years of Smoking: {_assessment.DurationOfSmoking} {_assessment.YearMonth}\n" +
                             $"Cigarettes/Day: {_assessment.CigarettesPerDay}\n" +
                             $"Cost per Pack: {_assessment.CigaretteCost.ToString("C", pesoCulture)}\n" +
                             $"Money Spent: {_savings.totalSaved.ToString("C", pesoCulture)}\n" +
                             $"Daily Savings: {_savings.Daily.ToString("C", pesoCulture)}\n" +
                             $"Weekly Savings: {_savings.Weekly.ToString("C", pesoCulture)}\n" +
                             $"Monthly Savings: {_savings.Monthly.ToString("C", pesoCulture)}\n" +
                             $"Yearly Savings: {(_savings.Daily * 365).ToString("C", pesoCulture)}\n" +
                             $"Confidence Level: {_assessment.ConfidenceLevel}";
        }


        // Logout Command
        private async Task LogoutUserAsync()
        {
            // Clear the current user session
            SessionManager.CurrentUser = null;  // Assuming you have a SessionManager

            // Optionally show confirmation
            await Application.Current.MainPage.DisplayAlert(
                "Logout",
                "You have been logged out successfully.",
                "OK"
            );

            // Navigate back to the Login page
            Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
        }
    }
}
