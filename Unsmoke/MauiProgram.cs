using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Unsmoke.MVVM.Views;
using CommunityToolkit.Maui;

namespace Unsmoke
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<Dashboard>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<Assessment>();
            builder.Services.AddSingleton<Progress>();
            builder.Services.AddTransient<CreatePost>();
            builder.Services.AddSingleton<Community>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddSingleton<MyPlan>();
            builder.Services.AddSingleton<ProfilePage>();
            builder.Services.AddSingleton<SplashScreen>();
            return builder.Build();
        }
    }
}
