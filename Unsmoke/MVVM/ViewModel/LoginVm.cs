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
using Unsmoke.Helper;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class LoginVm : ObservableObject
    {
        private readonly FirestoreService _firestoreService;
        [ObservableProperty]
        private Users user = new Users();

        [ObservableProperty]
        private bool isPassword = true;

        [ObservableProperty]
        private string eyeIcon = "eyeopen.svg";

        //Commands
        public ICommand LoginCommand { get; set; }
        public ICommand GotoRegister { get; set; }
        public ICommand TogglePassword { get; set; }
        public LoginVm() 
        {
            GotoRegister = new RelayCommand(RegisterPage);
            TogglePassword = new RelayCommand(TogglePasswordVisibility);
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            LoginCommand = new AsyncRelayCommand(LoginAsync);

            


        }



        public async void RegisterPage()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<RegisterPage>();
            return;
        }

        //Create toggle password
        private void TogglePasswordVisibility()
        {
            EyeIcon = IsPassword ? "eyeclose.svg" : "eyeopen.svg";
            IsPassword = !IsPassword;
        }
        // Login function with validations
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both username and password.", "OK");
                
                return;
            }

            try
            {
                var usersData = await _firestoreService.GetDocumentsAsync("Users");
                var data = JObject.Parse(usersData);
                var documents = data["documents"];

                if (documents == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No users found.", "OK");
                    ClearFields();
                    return;
                }

                foreach (var doc in documents)
                {
                    var username = doc["fields"]?["Username"]?["stringValue"]?.ToString();
                    var passwordHash = doc["fields"]?["Password"]?["stringValue"]?.ToString();

                    // Compare username
                    if (username == user.Username)
                    {
                        // Verify password hash
                        if (VerifyPassword(user.Password, passwordHash))
                        {
                            SessionManager.CurrentUser = new Users
                            {
                                UserID = user.UserID, // you'll get this from Firestore data
                                FullName = User.FullName,
                                Username = User.Username
                            };

                            await Application.Current.MainPage.DisplayAlert("Success", "Login successful!", "OK");
                            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
                            return;
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Incorrect password.", "OK");
                            ClearFields();
                            return;
                        }
                    }
                }

                await Application.Current.MainPage.DisplayAlert("Error", "Username not found.", "OK");
                ClearFields();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
           
        }

        // Password verification (hash)
        private bool VerifyPassword(string plainPassword, string storedHash)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword));
                var hash = Convert.ToBase64String(hashedBytes);
                return hash == storedHash;
            }
        }

        private void ClearFields()
        {
            User = new Users();
        }

    }
}
