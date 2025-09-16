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
using Unsmoke.MVVM.Models;
using Unsmoke.Service;


namespace Unsmoke.MVVM.ViewModel
{
    public partial class ProgressVM : ObservableObject
    {
        private readonly FirestoreService _firestoreService;
        public ObservableCollection<Achievement> Achievements { get; set; }

        public ObservableCollection<Item> Items { get; set; } = new();  // Store added items


        [ObservableProperty]
        private bool showprog = false;

        [ObservableProperty]
        private bool itemcomp = true;


        [ObservableProperty]
        private string itemName;   // Bound to Entry NAME

        [ObservableProperty]
        private string itemPrice;  // Bound to Entry PRICE

        [ObservableProperty]
        private string image = "add.svg";


        //Commands
        public ICommand ShowProgress { get; }
        public ICommand ItemComp { get; }
        public ICommand AddItemCommand { get; }
        public ICommand PickImageCommand { get; }

        //Constructor
        public ProgressVM()
        {
            Achievements = new ObservableCollection<Achievement>
                {
                    new Achievement { Title = "First Day", Description = "24 hours smoke-free", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Money Saver", Description = "Saved ₱100", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Health Hero", Description = "3 days clean", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Week Warrior", Description = "7 days smoke-free", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Strong Lungs", Description = "2 weeks clean", Icon = "unlock", IsUnlocked = false },
                    new Achievement { Title = "Champion", Description = "30 days milestone", Icon = "unlock", IsUnlocked = false },
                    // Add the rest of your achievements here...
                };

            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            ShowProgress = new RelayCommand(Progress);
            ItemComp = new RelayCommand(ShowItemComparison);
            AddItemCommand = new AsyncRelayCommand(AddItemAsync);
            PickImageCommand = new AsyncRelayCommand(PickImageAsync);
        }

        private int index = 1;

        // Show the Progress Section
        public void Progress()
        {
            Showprog = true;   // Show progress section
            Itemcomp = false;  // Hide item comparison section (optional)
        }
        public void ShowItemComparison()
        {
            Showprog = false;
            Itemcomp = true;
        }
        private async Task PickImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an Image"
                });

                if (result != null)
                {
                    Image = result.FullPath; // Set new image from device
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Image selection failed: {ex.Message}", "OK");
            }
        }

        // ADD ITEM FUNCTION
        private async Task AddItemAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(ItemName) || string.IsNullOrWhiteSpace(ItemPrice))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill out both fields", "OK");
                return;
            }

            if (!double.TryParse(ItemPrice, out double price))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Price must be a valid number", "OK");
                return;
            }

            // Create item object
            var newItem = new Item
            {
                ItemName = ItemName,
                ItemPrice = price,
                Image = image // Placeholder, implement image handling as needed
            };

            // Save to Firestore
            await _firestoreService.AddDocumentAsync("ItemComparison", newItem);

            // Add to local collection for UI
            Items.Add(newItem);

            await Application.Current.MainPage.DisplayAlert("Success", "Item Added!", "OK");
        }

        //Add Image
    }
}
