using System.Runtime.ExceptionServices;
using Unsmoke.MVVM.ViewModel;

namespace Unsmoke.MVVM.Views;

public partial class Assessment : ContentPage
{
    private readonly AssessmentViewModel? vm;
    public Assessment()
	{
        InitializeComponent();

        vm = BindingContext as AssessmentViewModel;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine($"CurrentImage on page appearing: {vm?.CurrentImage}");
    }
    private async void OnGenderImageClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton imageButton)
        {
            // Animate to the new scale (already set by ViewModel)
            await imageButton.ScaleTo(imageButton.Scale, 100, Easing.CubicIn);
        }
    }

    private void YearsOfSmokingEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && entry.Text == "0")
            entry.Text = string.Empty;
    }

    private void CigarettesPerDayEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && entry.Text == "0")
            entry.Text = string.Empty;
    }

    private void CigaretteCostEntry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && (entry.Text == "0" || entry.Text == "0.00"))
            entry.Text = string.Empty;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private void OnCustomPickerTapped(object sender, EventArgs e)
    {
        HiddenPicker.Focus(); // Open the native picker
        BackgroundColor.IsNotDefault();
    }

}