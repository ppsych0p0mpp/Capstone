using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public  class Education
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; } // optional if you want links

        public string Icon { get; set; } // Path to the icon image

    }
}
