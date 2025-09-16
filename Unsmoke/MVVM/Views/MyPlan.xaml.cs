using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.ViewModel;

namespace Unsmoke.MVVM.Views;

public partial class MyPlan : ContentPage
{
	public MyPlan()
	{
		InitializeComponent();
        BindingContext = new MyPlanVM();
    }

    private async void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Education resource && !string.IsNullOrWhiteSpace(resource.Url))
        {
            await Launcher.Default.OpenAsync(resource.Url);
        }

    // Deselect the item so it doesn't stay highlighted
    ((ListView)sender).SelectedItem = null;
    }
}