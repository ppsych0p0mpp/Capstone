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
using Unsmoke.Helper;

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
            try
            {
                var posts = await _firestoreService.GetDocumentsAsync<Post>("CommunityPosts");

                var users = await _firestoreService.GetDocumentsWithIdAsync<Users>("Users");

                var userDictionary = users.ToDictionary(u => u.UserID, u => u.FullName);

                var visiblePosts = posts
                    .Where(p => !p.IsDeleted) //Skip deleted posts
                    .OrderByDescending(p => p.DateCreated)
                    .ToList();

                foreach (var post in visiblePosts)
                {
                    if (!string.IsNullOrEmpty(post.UserId) && userDictionary.ContainsKey(post.UserId))
                        post.FullName = userDictionary[post.UserId];
                    else
                        post.FullName = "Unknown User"; // fallback if user not found
                }

                Posts.Clear();
                foreach (var post in visiblePosts)
                {
                    Posts.Add(post);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load posts: {ex.Message}", "OK");
            }
        }

        //function for Soft Deletion the post 
        private async Task DeletePostAsync(Post post)
        {
            if (!SessionManager.IsLoggedIn)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Login Required",
                    "You need to log in before deleting a post.",
                    "OK");
                return;
            }

            if (post == null) return;

            // ✅ Check if current user is the owner of the post
            if (post.UserId != SessionManager.CurrentUser?.UserID)
            {
               
                return;
            }

            // Confirm deletion
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                "Are you sure you want to delete this post?",
                "Yes",
                "No"
            );

            if (!confirm) return;

            // Set IsDeleted = true in Firestore
            var updateData = new { IsDeleted = true };
            await _firestoreService.UpdateDocumentAsync("CommunityPosts", post.Id, updateData);

            // Remove from local list
            Posts.Remove(post);

            await Application.Current.MainPage.DisplayAlert("Deleted", "Post has been deleted successfully.", "OK");
        }

        private async Task EditPostAsync(Post post)
        {
            if (!SessionManager.IsLoggedIn)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Login Required",
                    "You need to log in before editing a post.",
                    "OK");
                return;
            }

            if (post == null) return;

            // ✅ Check if current user is the owner of the post
            if (post.UserId != SessionManager.CurrentUser?.UserID)
            {
                
                return;
            }

            // Navigate to edit page with post data
            var editPage = App.Services.GetRequiredService<CreatePost>();
            (editPage.BindingContext as CreatePostVM)?.LoadPostForEditing(post);

            Application.Current.MainPage = editPage;
        }


        private async void Addpost()
        {
            try
            {
                // Check if user is logged in
                if (!SessionManager.IsLoggedIn || SessionManager.CurrentUser == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Login Required",
                        "You need to log in or register before creating a post.",
                        "OK");
                    return;
                }

                // Navigate to CreatePost page directly
                Application.Current.MainPage = App.Services.GetRequiredService<CreatePost>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to open post page: {ex.Message}", "OK");
            }
        }
       


    }
}
