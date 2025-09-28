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
            GetLocationButton.Text = "Konum Al�n�yor...";

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                // Web Mercator koordinat�na �evir
                var mercatorPoint = Mapsui.Projections.SphericalMercator.FromLonLat(
                    location.Longitude,
                    location.Latitude
                );

                var currPos = new Mapsui.MPoint(mercatorPoint.x, mercatorPoint.y);

                // Pin olu�tur ve ekle
                AddLocationPin(currPos, "Mevcut Konumum");

                // Bina seviyesinde yak�nla�t�r
                MV.Map.Navigator.CenterOnAndZoomTo(currPos, 1);

                GetLocationButton.Text = "Konum Al�nd�";
            }
            else
            {
                GetLocationButton.Text = "Konum Al�namad�";
            }
        }
        catch (Exception ex)
        {
            GetLocationButton.Text = "Hata";
        }
        finally
        {
            await Task.Delay(2000);
            GetLocationButton.Text = "Anl�k Pozisyonu Al";
            GetLocationButton.IsEnabled = true;
        }
    }
    private void AddLocationPin(Mapsui.MPoint position, string label)
    {
        if (MV.Pins.Count > 0)
            MV.Pins.RemoveAt(0);

        // GPS koordinatlar�na �evir
        var gpsCoordinate = Mapsui.Projections.SphericalMercator.ToLonLat(position.X, position.Y);

        // Pin olu�tur
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
