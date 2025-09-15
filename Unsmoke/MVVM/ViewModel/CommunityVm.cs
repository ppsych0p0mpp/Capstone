using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unsmoke.MVVM.Models;
using Unsmoke.MVVM.Views;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Unsmoke.Service;

namespace Unsmoke.MVVM.ViewModel
{
    public partial class CommunityVm : ObservableObject
    {
        private readonly FirestoreService _firestoreService;

        [ObservableProperty]
        private bool showEditDelete = false;

        //Commands
        public ICommand GotoCreatePost { get; }
        public IAsyncRelayCommand LoadPostsCommand { get; }
        public IAsyncRelayCommand<Post> DeletePostCommand { get; }
        public IAsyncRelayCommand<Post> EditPostCommand { get; }

        public ICommand ShowEditDeleteAction { get; }

        public ObservableCollection<Post> Posts { get; } = new();

        public CommunityVm()
        {
            GotoCreatePost = new RelayCommand(Addpost);
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            LoadPostsCommand = new AsyncRelayCommand(LoadPostsAsync);
            DeletePostCommand = new AsyncRelayCommand<Post>(DeletePostAsync);
            EditPostCommand = new AsyncRelayCommand<Post>(EditPostAsync);
            ShowEditDeleteAction = new RelayCommand(showDeleteEdit);

            Task.Run(LoadPostsAsync);
        }

        private void showDeleteEdit()
        {
            ShowEditDelete = true;
        }

        private async Task LoadPostsAsync()
        {
            Posts.Clear();
            var json = await _firestoreService.GetDocumentsAsync("CommunityPosts");

            var data = JObject.Parse(json);
            var documents = data["documents"];

            if (documents != null)
            {
                foreach (var doc in documents)
                {
                    var fields = doc["fields"];
                    var id = doc["name"].ToString().Split('/').Last();

                    var post = new Post
                    {
                        Id = id,
                        Content = fields?["Content"]?["stringValue"]?.ToString(),
                        Tags = fields?["Tags"]?["stringValue"]?.ToString(),
                        UserId = int.TryParse(fields?["UserId"]?["integerValue"]?.ToString(), out var userId) ? userId : 0,
                        DateCreated = DateTime.TryParse(fields?["DateCreated"]?["timestampValue"]?.ToString(), out var date)
                                        ? date : DateTime.MinValue,
                        IsDeleted = bool.TryParse(fields?["IsDeleted"]?["booleanValue"]?.ToString(), out var deleted) && deleted
                    };

                    // Only add if NOT deleted
                    if (!post.IsDeleted)
                        Posts.Add(post);
                }
            }
        }

        //function for Soft Deletion the post 
        private async Task DeletePostAsync(Post post)
        {
            if (post == null) return;

            // Ask user to confirm deletion
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this post?",
                "Yes",
                "No"
            );

            if (!confirm)
                return; // If user clicked "No", do nothing

            // Set IsDeleted = true in Firestore
            var updateData = new { IsDeleted = true };
            await _firestoreService.UpdateDocumentAsync("CommunityPosts", post.Id, updateData);

            // Remove from local collection
            Posts.Remove(post);

            await Application.Current.MainPage.DisplayAlert("Deleted", "Post has been deleted successfully.", "OK");
        }
        //function when Edit
        private async Task EditPostAsync(Post post)
        {
            if (post == null) return;

            // Navigate to edit page with post data
            var editPage = App.Services.GetRequiredService<CreatePost>();
            (editPage.BindingContext as CreatePostVM)?.LoadPostForEditing(post);

            //Add validation for editing post

            Application.Current.MainPage = editPage;
        }
        
        private async void Addpost()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<CreatePost>();
            return;
        }
        
        
    }
}
