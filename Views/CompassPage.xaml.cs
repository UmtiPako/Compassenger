using Compassenger.Models;
using Compassenger.Services;

namespace Compassenger.Views;

public partial class CompassPage : ContentPage
{
    private readonly CompassService _compass;
    private readonly LocationService _locationService;
    private readonly SimpleKalmanFilter _compassFilter;
    private CancellationTokenSource cts;

    private Waypoint _waypoint;
    private double? _currentLat;
    private double? _currentLon;
    private double distanceToTargetInMeters;

    public CompassPage(CompassService compass, LocationService locationService)
    {
        InitializeComponent();
        _compass = compass;
        _locationService = locationService;
        _compassFilter = new SimpleKalmanFilter(0.0);
        cts = new CancellationTokenSource();

        _waypoint = NavigationData.CurrentWaypoint;
        if (_waypoint != null)
        {
             _targetName.Text = _waypoint.Name ?? "Hedef Noktası";
        }

        _compass.HeadingChanged += OnHeadingChanged;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status == PermissionStatus.Granted)
        {
            if (Compass.IsSupported)
            {
                _compass.Start();
            }

            CalculateDistance();
            _ = RefreshCurrentLocation(cts.Token);
        }
        else
        {
            await DisplayAlert("Permission required",
                "Location permission is needed to use the compass and GPS.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _compass.Stop();
        cts?.Cancel();
        _compass.HeadingChanged -= OnHeadingChanged;
    }

    private async void OnHeadingChanged(object sender, double heading)
    {
        var filteredHeading = _compassFilter.Filter(heading);
        await UpdateArrow(filteredHeading);
    }

    private async Task RefreshCurrentLocation(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var loc = await _locationService.GetCurrentUserLocationAsync();
                if (loc != null)
                {
                    _currentLat = loc.Latitude;
                    _currentLon = loc.Longitude;
                }

                CalculateDistance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
            }

            await Task.Delay(200, token);
        }
    }

    private void CalculateDistance()
    {
        if (_currentLat == null || _currentLon == null || _waypoint == null)
            return;

        var distanceToTargetInKms = Location.CalculateDistance(
            new Location(_currentLat.Value, _currentLon.Value),
            new Location(_waypoint.Latitude, _waypoint.Longitude),
            DistanceUnits.Kilometers);

        var distanceToTarget = distanceToTargetInKms;
        var sign = "km";

        if (distanceToTargetInKms < 10)
        {
            distanceToTargetInMeters = distanceToTargetInKms * 1000;
            distanceToTarget = distanceToTargetInMeters;
            sign = "m";
        }

        _distance.Text = $"{distanceToTarget.ToString("N2")}{sign}";
    }

    private async Task UpdateArrow(double deviceHeads)
    {
        if (_currentLat == null || _currentLon == null || _waypoint == null)
            return;

        var current = new Location(_currentLat.Value, _currentLon.Value);
        var target = new Location(_waypoint.Latitude, _waypoint.Longitude);

        var bearing = LocationService.CalculateBearing(current, target);
        var rotation = (bearing - deviceHeads + 360) % 360;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _arrow.Rotation = rotation;
        });
    }
}