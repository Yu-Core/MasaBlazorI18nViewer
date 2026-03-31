using MasaBlazorI18nViewer.Rcl.Service;
using MasaBlazorI18nViewer.Rcl.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MasaBlazorI18nViewer.Gtk.Extensions
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
