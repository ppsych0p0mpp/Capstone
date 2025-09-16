using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unsmoke.MVVM.Models;

namespace Unsmoke.MVVM.ViewModel
{
    public class MyPlanVM
    {
        public ObservableCollection<Education> Resources { get; set; }
        public ICommand OpenLinkCommand { get; set; }

        public MyPlanVM()
        {
            Resources = new ObservableCollection<Education>
            {
                new Education
            {
                Title = "Understand the Risks and Effects of Smoking",
                Description = "Learn about the health risks of smoking from trusted health organizations.",
                Url = "https://www.cdc.gov/tobacco/basic_information/health_effects/index.htm"
            },
                 new Education
            {
                Title = "WHO: Tobacco Facts",
                Description = "Global facts and statistics about tobacco use.",
                Url = "https://www.who.int/news-room/fact-sheets/detail/tobacco"
            },
                new Education
            {
                Title = "National Cancer Institute: Risks of Smoking",
                Description = "Understand how smoking leads to various diseases including cancer.",
                Url = "https://www.cancer.gov/about-cancer/causes-prevention/risk/tobacco"
            },
                new Education
            {
                Title = "Understand the Risks and Effects of Smoking",
                Description = "Learn about the health risks of smoking from trusted health organizations.",
                Url = "https://www.cdc.gov/tobacco/basic_information/health_effects/index.htm"
            },
            new Education
            {
                Title = "WHO: Tobacco Facts",
                Description = "Global facts and statistics about tobacco use.",
                Url = "https://www.who.int/news-room/fact-sheets/detail/tobacco"
            },
            new Education
            {
                Title = "National Cancer Institute: Risks of Smoking",
                Description = "Understand how smoking leads to various diseases including cancer.",
                Url = "https://www.cancer.gov/about-cancer/causes-prevention/risk/tobacco"
            }

            };
            OpenLinkCommand = new Command<string>(async (url) => await Launcher.OpenAsync(url));
        }
    }

}
