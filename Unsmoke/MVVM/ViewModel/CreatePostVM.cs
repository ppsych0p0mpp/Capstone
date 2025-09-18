using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.Helper;
using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.Views;
using Unsmoke.Service;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class CreatePostVM : ObservableObject
    {
        private readonly FirestoreService _firestoreService;

        [ObservableProperty]
        private string content;

        [ObservableProperty]
        private string selectedtags;

        [ObservableProperty]
        private string editingPostId;
        public List<string> AvailableTags { get; } = new()
        {
            "#dailyupdate",
            "#help",
            "#tips",
            "#myjourney"
        };

        //Commands
        public ICommand GotoCommunity { get; }
        public IAsyncRelayCommand CreatePostCommand { get; }

        public CreatePostVM()
        {
            GotoCommunity = new RelayCommand(BacktoComm);
            CreatePostCommand = new AsyncRelayCommand(AddPostAsync);

            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
        }

        private async void BacktoComm()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            return;
        }

        private async Task AddPostAsync()
        {
            // Validation checks
            if (string.IsNullOrWhiteSpace(Content))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Post content cannot be empty.", "OK");
                return;
            }

            if (Content.Length < 20)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Post content must be at least 20 characters long.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Selectedtags))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please select a tag before posting.", "OK");
                return;
            }

            try
            {
                // Get user info safely
                var userId = SessionManager.CurrentUser?.UserID ?? "1";// fallback if no user is logged in

                var userFullName = SessionManager.CurrentUser?.FullName ?? "Guest";

                if (!string.IsNullOrWhiteSpace(EditingPostId))
                {
                    // Update existing post
                    var updateObj = new
                    {
                        Content = Content,
                        Tags = Selectedtags,
                        UserId = userId,
                        FullName = userFullName,
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.UpdateDocumentAsync("CommunityPosts", EditingPostId, updateObj);
                    await Application.Current.MainPage.DisplayAlert("Success", "Post updated.", "OK");

                    EditingPostId = null;
                }
                else
                {
                    // Create new post
                    var post = new Post
                    {
                        Content = Content,
                        Tags = Selectedtags,
                        UserId = userId,
                        FullName = userFullName,
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.AddDocumentAsync("CommunityPosts", post);
                    await Application.Current.MainPage.DisplayAlert("Success", "Post created.", "OK");
                }

                // Clear inputs
                Content = string.Empty;
                Selectedtags = null;

                // Navigate back to community page
                Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save post: {ex.Message}", "OK");
            }
        }


        public void LoadPostForEditing(Post post)
        {
            Content = post.Content;
            Selectedtags = post.Tags;
            EditingPostId= post.Id; // Store the ID so we update instead of creating a new post
        }
    }
}
