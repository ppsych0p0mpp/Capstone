using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class Post
    {
        public string Id { get; set; }

        public string Tags { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsDeleted { get; set; }   // For soft deletion
    }
}
