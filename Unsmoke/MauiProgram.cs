using Microsoft.Extensions.Logging;
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
            return builder.Build();
        }
    }
}
