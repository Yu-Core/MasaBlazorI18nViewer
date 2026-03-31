using global::Gtk;
using MasaBlazorI18nViewer.Rcl;
using Microsoft.AspNetCore.Components.WebView.Gtk;

namespace MasaBlazorI18nViewer.Gtk
{
    public class MainWindow : ApplicationWindow
    {
        public MainWindow(Application application, IServiceProvider serviceProvider)
            : base(new global::Gtk.Internal.ApplicationWindowHandle(global::Gtk.Internal.ApplicationWindow.New(application.Handle.DangerousGetHandle()), ownsHandle: false))
        {
            SetDefaultSize(1024, 768);

            this.OnDestroy += (o, e) =>
            {
                application.Quit();
            };

            var blazorWebView = new BlazorWebView();
            blazorWebView.HostPage = Path.Combine("wwwroot", "index.html");
            blazorWebView.Services = serviceProvider;
            blazorWebView.RootComponents.Add<Routes>("#app");

            this.SetChild(blazorWebView);
        }
    }
}
