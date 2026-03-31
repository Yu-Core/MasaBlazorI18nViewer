namespace MasaBlazorI18nViewer.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "MasaBlazorI18nViewer.Maui" };
        }
    }
}
