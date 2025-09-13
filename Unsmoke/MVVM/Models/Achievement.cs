﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class Achievement
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsUnlocked { get; set; }
    }
}
