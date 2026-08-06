using Microsoft.Extensions.Logging;
using Picross.Maui.Data;

namespace Picross.Maui
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

            // Add database as singleton
            builder.Services.AddSingleton<Database>();
            // Don't add ThemedPage as Transient, this allows us to avoid writing .. : base(Database db) each page

            return builder.Build();
        }
    }
}
