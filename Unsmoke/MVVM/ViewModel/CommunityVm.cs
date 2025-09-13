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
    public class CommunityVm : ObservableObject
    {
        private readonly FirestoreService _firestoreService;
        public ICommand GotoCreatePost { get; }
        public IAsyncRelayCommand LoadPostsCommand { get; }

        public ObservableCollection<Post> Posts { get; } = new();

        public CommunityVm()
        {
            GotoCreatePost = new RelayCommand(EditPost);
            _firestoreService = new FirestoreService("capstone-c5e34", "AIzaSyDH3bHUr5GDw78m3oJtOaddHoPjtnk5Yxc");
            LoadPostsCommand = new AsyncRelayCommand(LoadPostsAsync);

            Task.Run(LoadPostsAsync);
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

                    Posts.Add(new Post
                    {
                        Id = id,
                        Content = fields?["Content"]?["stringValue"]?.ToString(),
                        Tags = fields?["Tags"]?["stringValue"]?.ToString(),
                        UserId = int.TryParse(fields?["UserId"]?["integerValue"]?.ToString(), out var userId) ? userId : 0,
                        DateCreated = DateTime.TryParse(fields?["DateCreated"]?["timestampValue"]?.ToString(), out var date)
                                        ? date : DateTime.MinValue
                    });
                }
            }
        }
        //Edit or Add Post
        private async void EditPost()
        {
            Application.Current.MainPage = App.Services.GetRequiredService<CreatePost>();
            return;
        }
        
        
    }
}
