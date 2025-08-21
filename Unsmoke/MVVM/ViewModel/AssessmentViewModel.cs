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
        private string selectedGender;

        [ObservableProperty]
        private string smokingDurationValue; // only numbers from Entry



        private int currentIndex = 1;

        public ICommand Show => new RelayCommand(ShowNext);

        

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
                    IsFirstVisible = true;
                    await Task.Delay(1000); //Get Started
                    break;
                case 2:
                    IsSecondVisible = true; //select Gender Q1

                    //Check validation before showing the next one
                    if (currentIndex == 2 && string.IsNullOrEmpty(selectedGender))
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please select your gender before proceeding.", "OK");
                        return;
                    }

                    break;
                case 3:
                    IsThirdVisible = true;// Years of smoking Q2

                    // Validate Q2 before moving to Q3
                    if (string.IsNullOrEmpty(smokingDurationValue))
                    {
                        await Application.Current.MainPage.DisplayAlert("Required", "Please enter how long you've been smoking", "OK");
                        return;
                    }

                    break;
                case 4:
                    IsFourthVisible = true;// Cigarette perday Q3
                    break; 
                case 5:
                    IsFifthVisible = true;//Cigarette cost Q4
                    break;
                case 6:
                    IsSixthVisible = true;//Confident in quitting Q5
                    break;
                case 7:
                    IsSeventhVisible = true;
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
