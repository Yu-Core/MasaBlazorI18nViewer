using MasaBlazorI18nViewer.Rcl.Service;
using MasaBlazorI18nViewer.Rcl.Services;

namespace MasaBlazorI18nViewer.Maui.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<IStaticWebAssets, Services.StaticWebAssets>();
            services.AddSingleton<IPlatformIntegration, Services.PlatformIntegration>();
            return services;
        }
    }
}
