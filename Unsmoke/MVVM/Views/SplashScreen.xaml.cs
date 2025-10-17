using CommunityToolkit.Maui.Layouts;
using Microsoft.Maui.Controls;

namespace Unsmoke.MVVM.Views;

public partial class SplashScreen : ContentPage
{
	public SplashScreen()
	{

		InitializeComponent();
        StartTimer();

    }
    private void StartTimer()
    {
        TimeSpan splashDuration = TimeSpan.FromSeconds(3); // gif duration

        Dispatcher.StartTimer(splashDuration, () =>
        {
            Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
            return false; // Return false to stop the timer after execution
        });
    }


}