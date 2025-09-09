using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Unsmoke.MVVM.Views;
using Unsmoke.MVVM.Models;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class AssessmentViewModel : ObservableObject
    {
        [ObservableProperty] private bool isFirstVisible = true;
        [ObservableProperty] private bool isSecondVisible = false;
        [ObservableProperty] private bool isThirdVisible = false;
        [ObservableProperty] private bool isFourthVisible = false;
        [ObservableProperty] private bool isFifthVisible = false;
        [ObservableProperty] private bool isSixthVisible = false;
        [ObservableProperty] private bool isSeventhVisible = false;
        [ObservableProperty] private Models.Assessment assessment = new Models.Assessment();
        [ObservableProperty] private double maleImageScale = 1.0;
        [ObservableProperty] private double femaleImageScale = 1.0;


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


        private async void ShowNext()
        {
            switch (currentIndex)
            {
                case 1:
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
            }

            // Hide all
            IsFirstVisible = false;
            IsSecondVisible = false;
            IsThirdVisible = false;
            IsFourthVisible = false;
            IsFifthVisible = false;
            IsSixthVisible = false;
            IsSeventhVisible = false;

            // Show next
            currentIndex++;
            switch (currentIndex)
            {
                case 1: IsFirstVisible = true; break;
                case 2: IsSecondVisible = true; break;
                case 3: IsThirdVisible = true; break;
                case 4: IsFourthVisible = true; break;
                case 5: IsFifthVisible = true; break;
                case 6: IsSixthVisible = true; break;
                case 7: IsSeventhVisible = true; break;
            }

            if (currentIndex > 7)
            {
                var dashboard = App.Services.GetRequiredService<Dashboard>();
                Application.Current.MainPage = dashboard;
                return;
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
