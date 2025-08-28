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

        [ObservableProperty]
        private double maleImageScale = 1.0;

        [ObservableProperty]
        private double femaleImageScale = 1.0;

        public class ConfidenceIcon
        {
            public string Icon { get; set; }
            public string Text { get; set; }
        }

        public ObservableCollection<ConfidenceIcon> ConfidenceIcons { get; }
        private int _selectedConfidenceIndex;

        public int SelectedConfidenceIndex
        {
            get => _selectedConfidenceIndex;
            set
            {
                if (_selectedConfidenceIndex != value)
                {
                    _selectedConfidenceIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConfidenceText));
                }
            }
        }

        public string ConfidenceText => ConfidenceIcons.Count > SelectedConfidenceIndex
            ? ConfidenceIcons[SelectedConfidenceIndex].Text
            : string.Empty;

        public ICommand ContinueCommand { get; }

        private int currentIndex = 1;

        public ICommand Show { get; }

        public AssessmentViewModel()
        {
            ConfidenceIcons = new ObservableCollection<ConfidenceIcon>
            {
                new ConfidenceIcon { Icon = "sad_full.png", Text = "Not Confident" },
                new ConfidenceIcon { Icon = "poker_full.png", Text = "Somewhat Confident" },
                new ConfidenceIcon { Icon = "happy_full.png", Text = "Very Confident" }
            };
            ContinueCommand = new Command(OnContinue);
            SelectedConfidenceIndex = 1; // Default to middle

            Show = new RelayCommand(ShowNext);
        }

        private void OnContinue()
        {
            // Logic to proceed to next question or result
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
            // Validate current input before moving to the next question
            switch (currentIndex)
            {
                case 1:
                    // No validation needed for Get Started
                    break;
                case 2:
                    if (string.IsNullOrEmpty(assessment.Gender))
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please select your gender before continuing.", "OK");
                        return;
                    }
                    break;
                case 3:
                    if (string.IsNullOrEmpty(assessment.YearMonth))
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please select Months or Years before continuing.", "OK");
                        return;
                    }
                    if (assessment.YearsOfSmoking <= 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please enter your years/months of smoking before continuing.", "OK");
                        return;
                    }
                    break;
                case 4:
                    if (assessment.CigarettesPerDay <= 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarettes per day before continuing.", "OK");
                        return;
                    }
                    break;
                case 5:
                    if (assessment.CigaretteCost <= 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarette cost before continuing.", "OK");
                        return;
                    }
                    break;
                // Add more cases if you need validation for other steps
            }

            // Hide all
            IsFirstVisible = false;
            IsSecondVisible = false;
            IsThirdVisible = false;
            IsFourthVisible = false;
            IsFifthVisible = false;
            IsSixthVisible = false;
            IsSeventhVisible = false;

            // Show the next one
            currentIndex++;
            switch (currentIndex)
            {
                case 1:
                    IsFirstVisible = true;
                    break;
                case 2:
                    IsSecondVisible = true;
                    break;
                case 3:
                    IsThirdVisible = true;
                    break;
                case 4:
                    IsFourthVisible = true;
                    break;
                case 5:
                    IsFifthVisible = true;
                    break;
                case 6:
                    IsSixthVisible = true;
                    break;
                case 7:
                    IsSeventhVisible = true;
                    break;
            }

            if (currentIndex > 7)
            {
                var dashboard = App.Services.GetRequiredService<Dashboard>();
                Application.Current.MainPage = dashboard;
                return;
            }
        }      

        public ICommand SetGenderCommand => new RelayCommand<string>(gender =>
        {
            Assessment.Gender = gender;
            if (gender == "Male")
            {
                MaleImageScale = 1.2;
                FemaleImageScale = 1.0;
            }
            else
            {
                MaleImageScale = 1.0;
                FemaleImageScale = 1.2;
            }
        });

      
    }
}
