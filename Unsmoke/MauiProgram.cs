using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Unsmoke.MVVM.Views;

namespace Unsmoke
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
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
            builder.Services.AddSingleton<CreatePost>();
            builder.Services.AddSingleton<Community>();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<MyPlan>();
            return builder.Build();
        }
    }
}
