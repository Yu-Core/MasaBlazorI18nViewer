using MasaBlazorI18nViewer.Gtk;
using MasaBlazorI18nViewer.Gtk.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddGtkBlazorWebView();

services.AddMasaBlazor();
services.AddDependencyInjection();

var sp = services.BuildServiceProvider();

var app = new App(sp);
return app.Run(args);