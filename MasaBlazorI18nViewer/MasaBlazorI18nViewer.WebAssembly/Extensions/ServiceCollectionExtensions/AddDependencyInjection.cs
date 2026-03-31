using MasaBlazorI18nViewer.Rcl.Service;

namespace MasaBlazorI18nViewer.WebAssembly.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<IStaticWebAssets, Services.StaticWebAssets>();
            services.AddScoped<Rcl.Services.IPlatformIntegration, Services.PlatformIntegration>();
            return services;
        }
    }
}
