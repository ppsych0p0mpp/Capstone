using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
            // Create Post object
            try
            {
                if (!string.IsNullOrWhiteSpace(EditingPostId))
                {
                    // Update existing post
                    var updateObj = new
                    {
                        Content = Content,
                        Tags = Selectedtags,
                        // Optionally update DateModified or keep original DateCreated
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.UpdateDocumentAsync("CommunityPosts", EditingPostId, updateObj);

                    await Application.Current.MainPage.DisplayAlert("Success", "Post updated.", "OK");

                    // clear editing state
                    EditingPostId = null;
                }
                else
                {
                    // Create new post
                    var post = new Post
                    {
                        Content = Content,
                        Tags = Selectedtags,
                        UserId = 1,
                        DateCreated = DateTime.UtcNow
                    };

                    await _firestoreService.AddDocumentAsync("CommunityPosts", post);

                    await Application.Current.MainPage.DisplayAlert("Success", "Post created.", "OK");
                }

                // clear inputs
                Content = string.Empty;
                Selectedtags = null;

                // navigate back to community (or AppShell if you use that)
                Application.Current.MainPage = App.Services.GetRequiredService<Community>();

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
