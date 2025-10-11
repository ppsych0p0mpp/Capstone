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
            _firestoreService = new FirestoreService("capstoneunsmoke", "AIzaSyA2N8h7DJB9K7O3ozSS4boXHWSvbqG6tXY");
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        private async void RegisterPage()
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
                    var fullName = doc["fields"]?["FullName"]?["stringValue"]?.ToString();

                    // Get Firestore document ID as UserID (foreign key)
                    var nameToken = doc["name"]?.ToString();
                    string userId = null;
                    if (!string.IsNullOrEmpty(nameToken))
                    {
                        var parts = nameToken.Split('/');
                        userId = parts.LastOrDefault(); // actual Firestore document ID
                    }

                    // Match username
                    if (username == user.Username)
                    {
                        if (VerifyPassword(user.Password, passwordHash))
                        {
                            // Store session with Firestore Document ID
                            SessionManager.CurrentUser = new Users
                            {
                                UserID = userId,
                                FullName = fullName,
                                Username = username
                            };

                            // Check if this user already has an assessment using UserID as foreign key
                            var assessments = await _firestoreService.QueryDocumentsAsync<Models.Assessment>(
                                "assessments",
                                "UserID",
                                userId
                            );
                            if (assessments == null || !assessments.Any())
                            {
                                // No assessment → ask to take assessment first
                                await Application.Current.MainPage.DisplayAlert("Welcome", "Please complete your first assessment.", "OK");
                                Application.Current.MainPage = App.Services.GetRequiredService<Views.Assessment>();
                            }
                            else
                            {
                                // Assessment exists → go to dashboard
                                await Application.Current.MainPage.DisplayAlert("Success", "Login successful!", "OK");
                                Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
                            }

                            return;
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Incorrect password.", "OK");
                            user.Password = string.Empty;
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
                ClearFields();
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
