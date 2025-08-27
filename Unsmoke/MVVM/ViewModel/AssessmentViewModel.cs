using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.MVVM.Views;
using Unsmoke.MVVM.Models;

namespace Unsmoke.MVVM.ViewModel
{
   
    public partial class AssessmentViewModel : ObservableObject
    {

        [ObservableProperty]
        private bool isFirstVisible = true;

        [ObservableProperty]
        private bool isSecondVisible = false;

        [ObservableProperty]
        private bool isThirdVisible = false;

        [ObservableProperty]
        private bool isFourthVisible = false;

        [ObservableProperty]
        private bool isFifthVisible = false;

        [ObservableProperty]
        private bool isSixthVisible = false;

        [ObservableProperty]
        private bool isSeventhVisible = false;

        [ObservableProperty]
        private Models.Assessment assessment = new Models.Assessment();



        private int currentIndex = 1;

        public ICommand Show { get; }

        public AssessmentViewModel()
        {
            Show = new RelayCommand(ShowNext);
        }

       private async void Assess()
        {

            if (string.IsNullOrEmpty(assessment.Gender)) //Validtion for gender. The control is Radio button
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please select your gender before continuing.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(assessment.YearsOfSmoking.ToString()) || assessment.YearsOfSmoking <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your years/months of smoking before continuing.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(assessment.CigarettesPerDay.ToString()) || assessment.CigarettesPerDay <= 0 || !assessment.CigarettesPerDay.ToString().All(char.IsDigit))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarettes per day before continuing.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(assessment.CigaretteCost.ToString()) || assessment.CigaretteCost <= 0 || !assessment.CigaretteCost.ToString().All(char.IsDigit))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarette cost before continuing.", "OK");
                return;
            }
        }



        private async void ShowNext()
        {
            // Hide all
            IsFirstVisible = false;
            IsSecondVisible = false;
            IsThirdVisible = false;
            IsFourthVisible = false;
            IsFifthVisible = false;
            IsSixthVisible = false;
            IsSeventhVisible = false;

            

            // Show the next one
            switch (currentIndex)
            {
                case 1:
                    IsFirstVisible = true;//Get Started
                    break;
                case 2:
                    IsSecondVisible = true; //select Gender Q1
                    Assess(); // Call Assess method to validate inputs
                    break;
                case 3:
                    IsThirdVisible = true;// Years of smoking Q2
                    Assess(); // Call Assess method to validate inputs
                    break;
                case 4:
                    IsFourthVisible = true;// Cigarette perday Q3
                    Assess(); // Call Assess method to validate inputs
                    break;
                case 5:
                    IsFifthVisible = true;//Cigarette cost Q4
                    Assess(); // Call Assess method to validate inputs
                    break;
                case 6:
                    IsSixthVisible = true;//Confident in quitting Q5
                    Assess(); // Call Assess method to validate inputs
                    break;
                case 7:
                    IsSeventhVisible = true;
                    Assess(); // Call Assess method to validate inputs
                    break;
            }

            currentIndex++;
            if(currentIndex >= 7)
            {
                var dashboard = App.Services.GetRequiredService<Dashboard>();
                Application.Current.MainPage = dashboard;
                return;
            }
        }

        // Equality Converter for RadioButtons
      

    }
}
