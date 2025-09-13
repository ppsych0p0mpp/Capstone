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
        private string tags;

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
            var post = new Post
            {
                Content = Content,
                Tags = Tags,
                UserId = 1, // later you can replace with logged-in user id
                DateCreated = DateTime.UtcNow
            };

            await _firestoreService.AddDocumentAsync("CommunityPosts", post);

            // After saving, go back to Community page
            Application.Current.MainPage = App.Services.GetRequiredService<AppShell>();
        }
    }
}
