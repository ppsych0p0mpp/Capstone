using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unsmoke.MVVM.Models
{
    public class Withdrawal : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public ObservableCollection<string> Tips { get; set; }
        public string Icon { get; set; } // Path to the icon image

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Withdrawal()
        {
            Tips = new ObservableCollection<string>();
        }
    }
}
