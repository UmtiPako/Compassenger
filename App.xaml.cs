using Compassenger.Models;
using Compassenger.Services;
using Compassenger.Views;

namespace Compassenger
{
    public partial class App : Application
    {
        public App(IServiceProvider services)
        {
            InitializeComponent();

            var compass = services.GetRequiredService<CompassService>();
            var location = services.GetRequiredService<LocationService>();

            MainPage = new MainPage();
        }
    }
}
