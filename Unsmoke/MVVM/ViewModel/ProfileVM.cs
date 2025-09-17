using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.MVVM.Models;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class ProfileVM : ObservableObject
    {
        private readonly CultureInfo pesoCulture = new CultureInfo("en-PH");

        public ICommand GotoDash { get; }

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
            GotoDash = new RelayCommand(Backto);
            DisplayAssessment();
        }

        private void Backto()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            return;
        }

        //
        private void DisplayAssessment()
        {
            // Calculate total days based on whether the user chose Years or Months
            double daysSmoked = _assessment.YearMonth == "Years"
                ? _assessment.DurationOfSmoking * 365
                : _assessment.DurationOfSmoking * 30;  // Approximation for months

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
                             $"Monthly Savings: {_savings.Weekly.ToString("C", pesoCulture)}\n" +
                             $"Yearly Savings: {(_savings.Daily * 365).ToString("C", pesoCulture)}\n" +
                             $"Confidence Level: {_assessment.ConfidenceLevel}";
        }
    }
}
