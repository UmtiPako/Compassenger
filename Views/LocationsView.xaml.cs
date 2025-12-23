using Compassenger.Data;
using Compassenger.Models;
using System.Collections.ObjectModel;

namespace Compassenger.Views;

public partial class LocationsView : ContentPage
{
    private readonly Repo _repo;
    private ObservableCollection<Waypoint> _waypointsCollection;
    private int _skip = 0;
    private const int _pageSize = 20;
    private bool _isLoading = false;
    private bool _isMoreDataAvailable = true;

    public LocationsView()
    {
        InitializeComponent();
        _repo = IPlatformApplication.Current?.Services.GetService<Repo>();
        _waypointsCollection = new ObservableCollection<Waypoint>();
        LocationsCollection.ItemsSource = _waypointsCollection;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Sayfa her acildiginda listeyi sifirdan yukle (guncel kalmasi icin)
        await ReloadLocationsAsync();
    }

    private async Task ReloadLocationsAsync()
    {
        _skip = 0;
        _isMoreDataAvailable = true;
        _waypointsCollection.Clear();
        await LoadMoreLocationsAsync();
    }

    private async Task LoadMoreLocationsAsync()
    {
        if (_isLoading || !_isMoreDataAvailable || _repo == null) return;

        try
        {
            _isLoading = true;
            LoadingFooter.IsVisible = true;

            // Yapay bir gecikme ekleyerek yukleniyor animasyonunu gorebiliriz (opsiyonel)
            // await Task.Delay(500);

            var newItems = await _repo.GetWaypointsPagedAsync(_skip, _pageSize);

            if (newItems.Count == 0)
            {
                _isMoreDataAvailable = false;
            }
            else
            {
                foreach (var item in newItems)
                {
                    _waypointsCollection.Add(item);
                }

                _skip += newItems.Count;

                // Eger gelen veri sayfa boyutundan azsa, baska veri kalmamistir
                if (newItems.Count < _pageSize)
                {
                    _isMoreDataAvailable = false;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load Error: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            LoadingFooter.IsVisible = false;
        }
    }

    private async void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        await LoadMoreLocationsAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await ReloadLocationsAsync();
        LocationsRefreshView.IsRefreshing = false;
    }

    public async void OnNavigateToLocationClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Waypoint waypoint)
        {
            NavigationData.CurrentWaypoint = waypoint;
            
            var compassPage = Handler.MauiContext.Services.GetService<CompassPage>();
            await Navigation.PushModalAsync(compassPage);
        }
    }

    public async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Waypoint waypoint)
        {
            bool answer = await DisplayAlert("Sil", $"{waypoint.Name} konumunu silmek istediğinize emin misiniz?", "Evet", "Hayır");
            if (answer)
            {
                await _repo.RemoveWaypointAsync(waypoint.Name);
                await ReloadLocationsAsync();
            }
        }
    }

    public async void OnShareClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.CommandParameter is Waypoint waypoint)
        {
            var text = $"{waypoint.Name} konumunu seninle paylaşıyorum! - via Compassenger: Easy Compass & Location Saving App";
            var uri = $"https://www.google.com/maps/search/?api=1&query={waypoint.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{waypoint.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = $"{text}\n{uri}",
                Title = "Konum Paylaş"
            });
        }
    }
}