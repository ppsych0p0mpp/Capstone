using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.MVVM.Views;
using Unsmoke.MVVM.Models;
using Unsmoke.Service;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class RegisterVM : ObservableObject
    {
        private readonly FirestoreService _firestoreService;

        [ObservableProperty]
        private Users user = new Users();

        [ObservableProperty]
        private bool isPassword = true;

        [ObservableProperty]
        private string eyeIcon = "eyeopen.svg";

        //COmmands
        public ICommand GotoLogin { get; set; }
        public ICommand RegisterCommand { get; set; }
        public ICommand TogglePassword { get; set; }
        public RegisterVM()
        {
            GotoLogin = new RelayCommand(LoginPage);
            TogglePassword = new RelayCommand(TogglePasswordVisibility);
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            RegisterCommand = new AsyncRelayCommand(CreateAccountAsync);
        }
        public async void LoginPage()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
            return;
        }
        private void TogglePasswordVisibility()
        {
            EyeIcon = IsPassword ? "eyeclose.svg" : "eyeopen.svg";
            IsPassword = !IsPassword;
        }

        //Create function for Create account here
        private async Task CreateAccountAsync()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(user.FullName) ||
                string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                string.IsNullOrWhiteSpace(user.ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            // Check password strength
            if (!IsStrongPassword(user.Password))
            {
                await Application.Current.MainPage.DisplayAlert("Weak Password",
                    "Password must be at least 8 characters long and a number.", "OK");
                return;
            }
            // Check password match
            if (user.Password != user.ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match!", "OK");
                return;
            }
            // Check if username exists
            var existingUser = await _firestoreService.GetDocumentsAsync("Users");
            var data = JObject.Parse(existingUser);
            var documents = data["documents"];

            if (documents != null)
            {
                foreach (var doc in documents)
                {
                    var username = doc["fields"]?["Username"]?["stringValue"]?.ToString();
                    if (username == user.Username)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Username already exists!", "OK");
                        return;
                    }
                }
            }

            // Hash the password before saving
            string hashedPassword = HashPassword(user.Password);

            try
            {
                // Save user to Firestore
                await _firestoreService.AddDocumentAsync("Users", new
                {
                    FullName = user.FullName,
                    Username = user.Username,
                    Password = hashedPassword
                });

                await Application.Current.MainPage.DisplayAlert("Success", "Account created successfully!", "OK");
                Application.Current.MainPage = App.Services.GetRequiredService<LoginPage>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to create account: {ex.Message}", "OK");
            }
        }

        // Password hashing using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // Check password strength
        private bool IsStrongPassword(string password)
        {
            if (password.Length < 8) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            return true;
        }

        
    }
}
