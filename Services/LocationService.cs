using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compassenger.Services
{
    public class LocationService
    {


        public async Task<Location> GetCurrentUserLocationAsync()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            return await Geolocation.GetLocationAsync(request);
        }


        public static double CalculateBearing(Location from, Location to)
        {
            var lat1 = DegreesToRadians(from.Latitude);
            var lat2 = DegreesToRadians(to.Latitude);
            var lonDiff = DegreesToRadians(to.Longitude - from.Longitude);

            var y = Math.Sin(lonDiff) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lonDiff);

            return (RadiansToDegrees(Math.Atan2(y, x)) + 360) % 360;
        }

        #region Helpers
        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;
        private static double RadiansToDegrees(double rad) => rad * 180.0 / Math.PI;
        #endregion
    }
}
