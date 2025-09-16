using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unsmoke.MVVM.Models;

namespace Unsmoke.Helper
{
    public static class SessionManager
    {
        public static Users CurrentUser { get; set; } = null;

        public static bool IsLoggedIn => CurrentUser != null;

        
    }
}
