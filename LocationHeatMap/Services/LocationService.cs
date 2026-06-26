using LocationHeatMap.Models;
using Microsoft.Maui.Devices.Sensors;

namespace LocationHeatMap.Services
{
    public class LocationService
    {
        private readonly DatabaseService _db;
        private CancellationTokenSource? _cts;
        private bool _isTracking;

        public event EventHandler<LocationEntry>? LocationSaved;

        public LocationService(DatabaseService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public bool IsTracking => _isTracking;

        public async Task<bool> StartTrackingAsync()
        {
            if (_isTracking) return true;

            // Check and request permission
            var status = await Permissions
                .CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions
                    .RequestAsync<Permissions.LocationWhenInUse>();
            }

            // Start tracking regardless — emulator may return denied
            // but still provide location via Extended Controls
            _cts = new CancellationTokenSource();
            _isTracking = true;
            _ = Task.Run(() => PollLoopAsync(_cts.Token));
            return true;
        }

        public void StopTracking()
        {
            _cts?.Cancel();
            _isTracking = false;
        }

        private async Task PollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
                        GeolocationAccuracy.Lowest,
                        TimeSpan.FromSeconds(3));

                    var location = await Geolocation
                        .GetLocationAsync(request, token);

                    if (location is not null)
                    {
                        var entry = new LocationEntry
                        {
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            AccuracyMeters = location.Accuracy ?? 0,
                            RecordedAtUtc = DateTime.UtcNow
                        };

                        await _db.SaveLocationAsync(entry);

                        MainThread.BeginInvokeOnMainThread(
                            () => LocationSaved?.Invoke(this, entry));
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[GPS] {ex.Message}");
                }

                try { await Task.Delay(3000, token); }
                catch { break; }
            }

            _isTracking = false;
        }
    }
}