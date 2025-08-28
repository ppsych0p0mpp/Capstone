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
using System.Collections.ObjectModel;

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
        private bool backButton = false;

        [ObservableProperty]
        private Models.Assessment assessment = new Models.Assessment();

        public ObservableCollection<string> Gender { get; set; }




        private int currentIndex = 1;

        public ICommand Show { get; }
        public ICommand Back { get; }
        public ICommand secondbtnQ { get; }
        public ICommand thirdbtnQ { get; }
        public ICommand fourthbtnQ { get; }
        public ICommand fifthbtnQ { get; }
        public ICommand sixthbtnQ { get; }
        public ICommand seventhbtnQ { get; }

        public AssessmentViewModel()
        {
            Show = new RelayCommand(ShowNext);
            secondbtnQ = new RelayCommand(NextQ);
            thirdbtnQ = new RelayCommand(SecondQ);
            fourthbtnQ = new RelayCommand(ThirdQ);
            fifthbtnQ = new RelayCommand(FourthQ);
            Back = new RelayCommand(BackQ);

        }


        public async void NextQ()
        {
            //First Question Validation
            if (string.IsNullOrEmpty(assessment.Gender))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please select your gender.", "OK");
                return;
            }
            else
            {
                ShowNext();
            }

        }

        public async void SecondQ()
        {
            //Second Question Validation
            if (assessment.YearsOfSmoking <= 0 || string.IsNullOrEmpty(assessment.YearsOfSmoking.ToString()) || string.IsNullOrEmpty(assessment.YearMonth)
                )
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter how long you’ve been smoking.", "OK");
                return;
            }
            else
            {
                ShowNext();
            }
        }

        public async void ThirdQ()
        {
            //Third Question Validation
            if (assessment.CigarettesPerDay <= 0 || string.IsNullOrEmpty(assessment.CigarettesPerDay.ToString()))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarettes per day.", "OK");
                return;
            }
            else
            {
                ShowNext();
            }
        }

        public async void FourthQ()
        {
            //Fourth Question Validation
            if (assessment.CigaretteCost <= 0 || string.IsNullOrEmpty(assessment.CigaretteCost.ToString()))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarette cost.", "OK");
                return;
            }
            else
            {
                Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
                return;
            }
        }

       


        private void ShowNext()
        {
            // Hide all
            IsFirstVisible = false;
            IsSecondVisible = false;
            IsThirdVisible = false;
            IsFourthVisible = false;
            IsFifthVisible = false;
            IsSixthVisible = false;
            IsSeventhVisible = false;
            BackButton = false;

            switch (currentIndex)
            {
                case 1:
                    IsFirstVisible = true;
                    break;

                case 2:
                    IsSecondVisible = true;
                    BackButton = true;
                    break;
                case 3:
                    IsThirdVisible = true; // Years smoking Q2
                    BackButton = true;
                    break;
                case 4:
                    IsFourthVisible = true; // Cigarettes/day Q3
                    BackButton = true;
                    break;
                case 5:
                    IsFifthVisible = true; // Cigarette cost Q4
                    BackButton = true;
                    break;
                case 6:
                    IsSixthVisible = true; // Confidence Q5
                    BackButton = true;
                    break;
                case 7:
                    IsSeventhVisible = true; // Last page
                    break;
            }
            currentIndex++;
        }

        //Back button function
        private void BackQ()
        {
            // Prevent going back before the first screen
            if (currentIndex <= 2)
                return;

            // Step back
            currentIndex--;

            // Hide all first
            IsFirstVisible = false;
            IsSecondVisible = false;
            IsThirdVisible = false;
            IsFourthVisible = false;
            IsFifthVisible = false;
            IsSixthVisible = false;
            IsSeventhVisible = false;

            // Show the correct screen depending on currentIndex
            switch (currentIndex)
            {
                case 1:
                    IsFirstVisible = true;  // Get Started
                    break;
                case 2:
                    IsSecondVisible = true; // Gender Q1
                    break;
                case 3:
                    IsThirdVisible = true;  // Years of smoking Q2
                    break;
                case 4:
                    IsFourthVisible = true; // Cigarettes per day Q3
                    break;
                case 5:
                    IsFifthVisible = true;  // Cigarette cost Q4
                    break;
                case 6:
                    IsSixthVisible = true;  // Confidence Q5
                    break;
                case 7:
                    IsSeventhVisible = true; // Final
                    break;
            }
        }

    }
}
