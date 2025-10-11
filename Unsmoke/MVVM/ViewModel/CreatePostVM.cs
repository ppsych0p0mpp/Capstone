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

            _firestoreService = new FirestoreService("capstoneunsmoke", "AIzaSyA2N8h7DJB9K7O3ozSS4boXHWSvbqG6tXY");
        }

        private async void BacktoComm()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
            return;
        }

        private async Task AddPostAsync()
        {
            // 1. Validate post content
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

            // 2. Ensure user is logged in
            if (SessionManager.CurrentUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You must be logged in to post.", "OK");
                return;
            }

            try
            {
                // Get user info
                var userId = SessionManager.CurrentUser.UserID;
                var userFullName = SessionManager.CurrentUser.FullName;

                if (!string.IsNullOrWhiteSpace(EditingPostId))
                {
                    // 3. Update existing post
                    var updateObj = new
                    {
                        Content = Content,
                        Tags = Selectedtags,
                        UserId = userId,
                        FullName = userFullName,
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.UpdateDocumentAsync("CommunityPosts", EditingPostId, updateObj);
                    await Application.Current.MainPage.DisplayAlert("Success", "Post updated successfully.", "OK");
                    EditingPostId = null;
                }
                else
                {
                    // 4. Create new post
                    var post = new Post
                    {
                        Id = Guid.NewGuid().ToString(),
                        Content = Content,
                        Tags = Selectedtags,
                        UserId = userId,
                        FullName = userFullName,
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.AddDocumentAsync("CommunityPosts", post);
                    await Application.Current.MainPage.DisplayAlert("Success", "Post created successfully.", "OK");
                }

                // 5. Clear inputs after posting
                Content = string.Empty;
                Selectedtags = null;

                // 6. Navigate back to Community page inside AppShell
               Application.Current.MainPage = App.Services.GetRequiredService<AppShell>(); ;
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
