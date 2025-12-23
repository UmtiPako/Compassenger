using Compassenger.Data;
using Compassenger.Models;
using Compassenger.Services;
using Mapsui;
using Mapsui.UI.Maui;
using System.Diagnostics;

namespace Compassenger.Views;

public partial class MapView : ContentPage
{
    private readonly Repo repo;
    private readonly LocationService _locationService;

    public MapView()
    {
        InitializeComponent();
        this.repo = IPlatformApplication.Current?.Services.GetService<Repo>();
        this._locationService = IPlatformApplication.Current?.Services.GetService<LocationService>();
        _ = InitializeMapAsync();
    }

    private async Task InitializeMapAsync()
    {
        if (MV.Map == null) return;
        
        MV.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        
        // Haritaya tiklama olayini dinle
        MV.MapClicked += OnMapClicked;
        
        await SetCurrentLocationAsync();
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        // Tiklanan nokta e.Point icinde gelir (MPoint tipinde ve harita projeksiyonunda)
        var point = e.Point;

        if (point != null)
        {
            // UI islemleri ana thread'de yapilmali
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddLocationPin(point);
            });
        }
    }

    private async Task SetCurrentLocationAsync()
    {
            var location = await _locationService.GetCurrentUserLocationAsync();

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

    private (double, double) currentCoordinates = (0, 0);
    private async void OnGetLocationClicked(object sender, EventArgs e)
    {
        try
        {
            GetLocationButton.IsEnabled = false;
            GetLocationButton.Text = "Konum Alınıyor...";

            var location = await _locationService.GetCurrentUserLocationAsync();

            if (location != null)
            {
                // Web Mercator koordinatına çevir
                var mercatorPoint = Mapsui.Projections.SphericalMercator.FromLonLat(
                    location.Longitude,
                    location.Latitude
                );

                var currPos = new Mapsui.MPoint(mercatorPoint.x, mercatorPoint.y);

                // Pin oluştur ve ekle
                AddLocationPin(new Position(location.Latitude, location.Longitude));

                // Bina seviyesinde yakınlaştır
                MV.Map.Navigator.CenterOnAndZoomTo(currPos, 1);

                GetLocationButton.Text = "Konum Alındı";
            }
            else
            {
                GetLocationButton.Text = "Konum Alınamadı";
            }
        }
        catch (Exception ex)
        {
            GetLocationButton.Text = "Hata";
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            await Task.Delay(2000);
            GetLocationButton.Text = "Anlık Pozisyonu Al";
            GetLocationButton.IsEnabled = true;
        }
    }

    private void AddLocationPin(Mapsui.UI.Maui.Position position)
    {
        if (MV.Pins.Count > 0)
            MV.Pins.RemoveAt(0);

        var pin = new Mapsui.UI.Maui.Pin
        {
            Position = position,
            Label = "Current",
            Type = Mapsui.UI.Maui.PinType.Pin
        };

        currentCoordinates = (position.Latitude, position.Longitude);

        MV.Pins.Add(pin);

        PinInfoCard.IsVisible = true;
        LocationNameEntry.Text = "";

        System.Diagnostics.Debug.WriteLine($"{position.Longitude}, {position.Latitude}");
    }

    private async void OnSaveLocationClicked(object sender, EventArgs e)
    {
        try
        {
            if (repo == null)
            {
                var services = IPlatformApplication.Current?.Services;
                if (services != null)
                {
                     var r = services.GetService<Repo>();
                     if(r == null) 
                     {
                         await DisplayAlert("Hata", "Veritabanı servisine erişilemiyor.", "Tamam");
                         return;
                     }
                }
            }

            if (currentCoordinates == (0, 0))
            {
                await DisplayAlert("Hata", "Lütfen önce bir konum seçin.", "Tamam");
                return;
            }

            if (string.IsNullOrWhiteSpace(LocationNameEntry.Text))
            {
                await DisplayAlert("Uyarı", "Lütfen lokasyon için bir isim girin.", "Tamam");
                return;
            }

            var waypoint = new Waypoint
            {
                Latitude = currentCoordinates.Item1,
                Longitude = currentCoordinates.Item2,
                Name = LocationNameEntry.Text
            };
            
            var activeRepo = repo ?? IPlatformApplication.Current?.Services.GetService<Repo>();

            if (activeRepo == null)
            {
                 await DisplayAlert("Hata", "Repo servisi başlatılamadı.", "Tamam");
                 return;
            }

            var result = await activeRepo.AddWaypointAsync(waypoint);
            
            if (result.IsSuccess)
            {
                await DisplayAlert("Başarılı", "Lokasyon kaydedildi.", "Tamam");
                PinInfoCard.IsVisible = false;
                LocationNameEntry.Text = "";
                MV.Pins.Clear();
            }
            else
            {
                await DisplayAlert("Hata", $"Lokasyon kaydedilirken hata: {result.ErrorMessage}", "Tamam");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Hata: " + ex.Message);
            await DisplayAlert("Hata", $"Beklenmedik bir hata oluştu: {ex.Message}", "Tamam");
        }
    }

    private async void OnStartDirectlyClicked(object sender, EventArgs e)
    {
        if (currentCoordinates == (0, 0)) return;

        var waypoint = new Waypoint
        {
            Latitude = currentCoordinates.Item1,
            Longitude = currentCoordinates.Item2,
            Name = null
        };

        NavigationData.CurrentWaypoint = waypoint;

        var compassPage = Handler.MauiContext.Services.GetService<CompassPage>();
        await Navigation.PushModalAsync(compassPage);
    }

    private void OnCancelPinClicked(object sender, EventArgs e)
    {
        MV.Pins.Clear();
        PinInfoCard.IsVisible = false;
        NavigationData.CurrentWaypoint = null;
    }
}

public static class NavigationData
{
    public static Waypoint CurrentWaypoint { get; set; }
}