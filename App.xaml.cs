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

            var waypoint = new Waypoint
            {
                Name = "Yurt",
                Latitude = 39.924984,
                Longitude = 32.836911
            };

            MainPage = new CompassPage(compass, location, waypoint);
        }
    }
}
