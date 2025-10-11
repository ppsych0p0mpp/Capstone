using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
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
    public partial class ProfileVM : ObservableObject
    {
        public ObservableCollection<Currency> AvailableCurrencies { get; }
        private Dictionary<string, double> _conversionRates = new();

        [ObservableProperty]
        Currency selectedCurrency;

        private string _baseCurrency = "PHP";  // or your default base

        private readonly FirestoreService __firestoreService;

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

        [ObservableProperty]
        private int streakDays;

        [ObservableProperty]
        private DashboardData dashdata = new DashboardData();

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
            __firestoreService = new FirestoreService("capstoneunsmoke", "AIzaSyA2N8h7DJB9K7O3ozSS4boXHWSvbqG6tXY");
            Logout = new AsyncRelayCommand(LogoutUserAsync);
            AvailableCurrencies = new ObservableCollection<Currency>(Currency.SupportedCurrencies);
            SelectedCurrency = AvailableCurrencies[0];
            FetchAndSetRatesAsync(_baseCurrency).ConfigureAwait(false);
            Task.Run(DisplayAssessmentAsync);
        }


        //Display Assessment Summary
        private async Task DisplayAssessmentAsync()
        {
            if (SessionManager.CurrentUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please login first to view your assessment.", "OK");
                return;
            }

            var userId = SessionManager.CurrentUser.UserID;
            FullName = SessionManager.CurrentUser.FullName;

            var assessments = await __firestoreService.QueryDocumentsAsync<Models.Assessment>(
                "assessments",
                "UserID", userId
            );

            _assessment = assessments.FirstOrDefault();

            if (_assessment == null)
            {
                await Application.Current.MainPage.DisplayAlert("No Data", "No assessment found for this user.", "OK");
                return;
            }

            AssessmentDate = _assessment.DateTaken.ToString("MMMM dd, yyyy");

            // Initially update the summary in default currency
            UpdateAllCurrencyDisplays();
        }


        // Logout Command
        private async Task LogoutUserAsync()
        {
            // Clear the current user session
            SessionManager.CurrentUser = null;

            // Optionally show confirmation
            await Application.Current.MainPage.DisplayAlert(
                "Logout",
                "You have been logged out successfully.",
                "OK"
            );

            // Navigate back to the Login page
            Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
        }

        //Function for change Currency
        partial void OnSelectedCurrencyChanged(Currency value)
        {
            if (value != null)
            {
                UpdateAllCurrencyDisplays();
            }
        }

        private async Task FetchAndSetRatesAsync(string baseCurrency)
        {
            string url = $"https://v6.exchangerate-api.com/v6/cdd7ad568b497f3486468ac2/latest/{baseCurrency}";
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);
            var apiObj = JsonConvert.DeserializeObject<API_Obj>(json);
            _conversionRates = apiObj.conversion_rates.ToDictionary();
            UpdateAllCurrencyDisplays();
        }

        private void UpdateAllCurrencyDisplays()
        {
            if (_assessment == null) return;

            double rate = 1.0;
            if (SelectedCurrency != null && _conversionRates.ContainsKey(SelectedCurrency.Code))
                rate = _conversionRates[SelectedCurrency.Code];

            var culture = new CultureInfo(SelectedCurrency.CultureCode);

            double dailyCost = _assessment.CigarettesPerDay * _assessment.CigaretteCost;
            double dailyCostConverted = dailyCost * rate;

            double daysSmoked = _assessment.YearMonth == "Years"
                ? _assessment.DurationOfSmoking * 365
                : _assessment.DurationOfSmoking * 30;

            double moneySpentConverted = dailyCostConverted * daysSmoked;
            double dailySavings = dailyCostConverted;
            double weeklySavings = dailyCostConverted * 7;
            double monthlySavings = dailyCostConverted * 30;
            double yearlySavings = dailyCostConverted * 365;

            SummaryMessage =
                $"Gender: {_assessment.Gender}\n" +
                $"Years of Smoking: {_assessment.DurationOfSmoking} {_assessment.YearMonth}\n" +
                $"Cigarettes/Day: {_assessment.CigarettesPerDay}\n" +
                $"Cost per Pack: {dailyCostConverted.ToString("C", culture)}\n" +
                $"Money Spent: {moneySpentConverted.ToString("C", culture)}\n" +
                $"Daily Savings: {dailySavings.ToString("C", culture)}\n" +
                $"Weekly Savings: {weeklySavings.ToString("C", culture)}\n" +
                $"Monthly Savings: {monthlySavings.ToString("C", culture)}\n" +
                $"Yearly Savings: {yearlySavings.ToString("C", culture)}\n" +
                $"Confidence Level: {_assessment.ConfidenceLevel}";

            OnPropertyChanged(nameof(SummaryMessage));
        }

        // API response classes
        public class API_Obj
        {
            public string result { get; set; }
            public Dictionary<string, double> conversion_rates { get; set; }

            public Dictionary<string, double> ToDictionary() => conversion_rates;
        }
    }
}
