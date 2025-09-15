﻿using Unsmoke.MVVM.Views;

namespace Unsmoke
{
    public partial class App : Application
    {
        public static IServiceProvider? Services { get; private set; }
        public App( IServiceProvider serviceProvider)
        {

            InitializeComponent();
            Services = serviceProvider;
            MainPage = App.Services.GetRequiredService<LoginPage>();
        }
    }
}
