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
using Unsmoke.Service;

namespace Unsmoke.MVVM.ViewModel
{
   
    public partial class AssessmentViewModel : ObservableObject
    {
        private readonly FirestoreService _firestoreService;

        [ObservableProperty]
        private Users _user = new Users();

        [ObservableProperty]
        private Models.Savings _savings = new Models.Savings();

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
        private bool dashlines = false;

        public ObservableCollection<string> DashlineColors { get; set; }
         = new ObservableCollection<string> { "#2E2E2E", "#2E2E2E", "#2E2E2E", "#2E2E2E", "#2E2E2E" };

        [ObservableProperty]
        private Models.Assessment assessment = new Models.Assessment();

        [ObservableProperty] 
        private double maleImageScale = 1.0;

        [ObservableProperty] 
        private double femaleImageScale = 1.0;

        [ObservableProperty]
        private string currentImage = "loading.gif";  // default image

        [ObservableProperty]
        private string headerText = "Preparing your results…";

        [ObservableProperty]
        private bool showFinishButton = false;

        [ObservableProperty]
        private bool isLoadingAnimationPlaying = false;

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


        private Color _maleBackgroundColor = Colors.Gray;
        public Color MaleBackgroundColor
        {
            get => _maleBackgroundColor;
            set => SetProperty(ref _maleBackgroundColor, value);
        }

        private Color _femaleBackgroundColor = Colors.Gray;
        public Color FemaleBackgroundColor
        {
            get => _femaleBackgroundColor;
            set => SetProperty(ref _femaleBackgroundColor, value);
        }

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

        public ICommand GotoLogin { get; }
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
            System.Diagnostics.Debug.WriteLine($"VM created, CurrentImage default: {CurrentImage}");
            ConfidenceIcons = new ObservableCollection<ConfidenceIcon>
            {
                new ConfidenceIcon { Icon = "sad.svg", Text = "Not Confident" },
                new ConfidenceIcon { Icon = "confused.svg", Text = "Somewhat Confident" },
                new ConfidenceIcon { Icon = "happy.svg", Text = "Very Confident" }
            };
            SelectedConfidenceIndex = 1; // Default to middle

            Show = new RelayCommand(ShowNext);
            secondbtnQ = new RelayCommand(NextQ);
            thirdbtnQ = new RelayCommand(SecondQ);
            fourthbtnQ = new RelayCommand(ThirdQ);
            fifthbtnQ = new RelayCommand(FourthQ);
            sixthbtnQ = new RelayCommand(FifthQ);
            seventhbtnQ = new RelayCommand(ResultQ);
            Back = new RelayCommand(BackQ);
            GotoLogin = new RelayCommand(LoginP);
 
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
        }
        
        private async void LoginP()
        {
           Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
           return;
        }

        public async void NextQ()
        {
            //First Question Validation
            if (string.IsNullOrEmpty(Assessment.Gender))
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
            if (Assessment.DurationOfSmoking <= 0 || string.IsNullOrEmpty(Assessment.DurationOfSmoking.ToString()) || string.IsNullOrEmpty(assessment.YearMonth)
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
            if (Assessment.CigarettesPerDay <= 0 || string.IsNullOrEmpty(Assessment.CigarettesPerDay.ToString()))
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
            if (Assessment.CigaretteCost <= 0 || string.IsNullOrEmpty(Assessment.CigaretteCost.ToString()))
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please enter your cigarette cost.", "OK");
                return;
            }
            else
            {
                ShowNext();
            }
        }

        // Fifth Question Validation
        public async void FifthQ()
        {
            // Validate the confidence level
            if (SelectedConfidenceIndex < 0 || SelectedConfidenceIndex >= ConfidenceIcons.Count)
            {
                await Application.Current.MainPage.DisplayAlert("Required", "Please select your confidence level before continuing.", "OK");
                return;
            }

            // Store the selected confidence text in the assessment
            Assessment.ConfidenceLevel = ConfidenceIcons[SelectedConfidenceIndex].Text;

            // If validation passes, go to next question
            ShowNext();
        }

        public async void ResultQ()
        {
            // Save assessment data to Firestore when reaching final page
            try
            {   
                var newAssessment = new
                {
                    AssessmentID = Assessment.AssessmentID = Guid.NewGuid().ToString(),
                    UserId = _user.UserID,
                    DateTaken = Assessment.DateTaken = DateTime.UtcNow,
                    Gender = Assessment.Gender,
                    YearsOfSmoking = Assessment.DurationOfSmoking,
                    YearMonth = Assessment.YearMonth,
                    CigarettesPerDay = Assessment.CigarettesPerDay,
                    CigaretteCost = Assessment.CigaretteCost,
                    ConfidenceLevel = Assessment.ConfidenceLevel
                };

                await _firestoreService.AddDocumentAsync("assessments", newAssessment);
                await Application.Current.MainPage.DisplayAlert("Success", "Your assessment was saved!", "OK");

                // Navigate to main page after saving
                Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save assessment: " + ex.Message, "OK");
            }
            return;
        }

        private int currentIndex = 1;

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
            BackButton = false;
            Dashlines = false;
            switch (currentIndex)
            {
                case 1:
                    IsFirstVisible = true;
                    break;

                case 2:
                    IsSecondVisible = true;
                    BackButton = true;
                    Dashlines = true;
                    //Dashline color change to active
                    break;
                case 3:
                    IsThirdVisible = true; // Years smoking Q2
                    BackButton = true;
                    Dashlines = true;
                    break;
                case 4:
                    IsFourthVisible = true; // Cigarettes/day Q3
                    BackButton = true;
                    Dashlines = true;
                    break;
                case 5:
                    IsFifthVisible = true; // Cigarette cost Q4
                    BackButton = true;
                    Dashlines = true;
                    break;
                case 6:
                    IsSixthVisible = true; // Confidence Q5
                    BackButton = true;
                    Dashlines = true;
                    break;
                case 7:
                    System.Diagnostics.Debug.WriteLine($"Reached case 7, CurrentImage before change: {CurrentImage}");
                    IsSeventhVisible = true; // Last page

                    // Start the loading animation
                    HeaderText = "Preparing your results…";
                    CurrentImage = "loading.gif";      // MUST exist in Resources/Images
                    IsLoadingAnimationPlaying = true;  // bind this in XAML to Image.IsAnimationPlaying
                    ShowFinishButton = false;

                    // Small yield so UI processes binding change
                    await Task.Yield();

                    // Wait for the GIF to "play" (adjust duration as needed)
                    await Task.Delay(6000);

                    // After delay, stop animation and show check icon
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        IsLoadingAnimationPlaying = false;
                        CurrentImage = "check.svg";
                        HeaderText = "Assessment Complete!";
                        ShowFinishButton = true;
                    });

                    return;
            }
            // Update active dashline color (starting at 2nd frame)
            if (currentIndex >= 2 && currentIndex - 2 < DashlineColors.Count)
            {
                DashlineColors[currentIndex - 2] = "#FFC000";  // Active color
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
            // Reset the color when going back
            if (currentIndex >= 2 && currentIndex - 1 < DashlineColors.Count)
            {
                DashlineColors[currentIndex - 1] = "#2E2E2E"; // Reset to default color
            }
        }

        // Unified Gender Command (handles both scale and border colors)
        public ICommand SetGenderCommand => new RelayCommand<string>(gender =>
        {
            Assessment.Gender = gender;

            if (gender == "Male")
            {
                MaleImageScale = 1.5;
                FemaleImageScale = 1.0;
                MaleBackgroundColor = Color.FromArgb("#FFC000");
                FemaleBackgroundColor = Colors.Gray;
            }
            else if (gender == "Female")
            {
                MaleImageScale = 1.0;
                FemaleImageScale = 1.5;
                MaleBackgroundColor = Colors.Gray;
                FemaleBackgroundColor = Color.FromArgb("#FFC000");
            }
            OnPropertyChanged(nameof(MaleBackgroundColor));
            OnPropertyChanged(nameof(FemaleBackgroundColor));
            OnPropertyChanged(nameof(MaleImageScale));
            OnPropertyChanged(nameof(FemaleImageScale));
        });
        
       

        

    }
}
