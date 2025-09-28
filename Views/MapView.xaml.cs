using Compassenger.Services;
using Mapsui;

namespace Compassenger.Views;

public partial class MapView : ContentPage
{
    public MapView()
    {
        InitializeComponent();
        _ = InitializeMapAsync();
    }

    private async Task InitializeMapAsync()
    {
        MV.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        await SetCurrentLocationAsync();
    }

    private async Task SetCurrentLocationAsync()
    {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                var mercatorPoint = Mapsui.Projections.SphericalMercator.FromLonLat(
                location.Longitude,
                location.Latitude
                );

                var centerPoint = new Mapsui.MPoint(mercatorPoint.x, mercatorPoint.y);

                MV.Map.Navigator.CenterOnAndZoomTo(centerPoint, 1);
        }
    }

    private async void OnGetLocationClicked(object sender, EventArgs e)
    {
        try
        {
            GetLocationButton.IsEnabled = false;
            GetLocationButton.Text = "Konum Alýnýyor...";

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                // Web Mercator koordinatýna çevir
                var mercatorPoint = Mapsui.Projections.SphericalMercator.FromLonLat(
                    location.Longitude,
                    location.Latitude
                );

                var currPos = new Mapsui.MPoint(mercatorPoint.x, mercatorPoint.y);

                // Pin oluþtur ve ekle
                AddLocationPin(currPos, "Mevcut Konumum");

                // Bina seviyesinde yakýnlaþtýr
                MV.Map.Navigator.CenterOnAndZoomTo(currPos, 1);

                GetLocationButton.Text = "Konum Alýndý";
            }
            else
            {
                GetLocationButton.Text = "Konum Alýnamadý";
            }
        }
        catch (Exception ex)
        {
            GetLocationButton.Text = "Hata";
        }
        finally
        {
            await Task.Delay(2000);
            GetLocationButton.Text = "Anlýk Pozisyonu Al";
            GetLocationButton.IsEnabled = true;
        }
    }
    private void AddLocationPin(Mapsui.MPoint position, string label)
    {
        if (MV.Pins.Count > 0)
            MV.Pins.RemoveAt(0);

        // GPS koordinatlarýna çevir
        var gpsCoordinate = Mapsui.Projections.SphericalMercator.ToLonLat(position.X, position.Y);

        // Pin oluþtur
        var pin = new Mapsui.UI.Maui.Pin
        {
            Position = new Mapsui.UI.Maui.Position(gpsCoordinate.lat, gpsCoordinate.lon), // Lat, Lon
            Label = label,
            Type = Mapsui.UI.Maui.PinType.Pin
        };

        MV.Pins.Add(pin);

        System.Diagnostics.Debug.WriteLine($"{label}: {gpsCoordinate.lon}, {gpsCoordinate.lat}");
    }
}
