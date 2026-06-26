using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LocationHeatMap.Models;
using LocationHeatMap.Services;

namespace LocationHeatMap.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private readonly LocationService _locationService;
        private bool _isTracking;
        private string _statusMessage = "Tap ▶ to start tracking";
        private int _pointCount;

        public MainViewModel(DatabaseService db, LocationService locationService)
        {
            _db = db;
            _locationService = locationService;

            _locationService.LocationSaved += OnLocationSaved;

            ToggleTrackingCommand = new Command(
                async () => await ToggleTrackingAsync());
            ClearDataCommand = new Command(
                async () => await ClearDataAsync(),
                () => PointCount > 0);
            AddManualPointCommand = new Command(
                async () => await AddManualPointAsync());
        }

        public bool IsTracking
        {
            get => _isTracking;
            private set => SetField(ref _isTracking, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetField(ref _statusMessage, value);
        }

        public int PointCount
        {
            get => _pointCount;
            private set
            {
                if (SetField(ref _pointCount, value))
                    (ClearDataCommand as Command)?.ChangeCanExecute();
            }
        }

        public string TrackingButtonLabel =>
            IsTracking ? "⏹ Stop" : "▶ Start";

        public ObservableCollection<LocationEntry> Locations { get; } = new();

        public ICommand ToggleTrackingCommand { get; }
        public ICommand ClearDataCommand { get; }
        public ICommand AddManualPointCommand { get; }

        public async Task InitialiseAsync()
        {
            await _db.InitAsync();
            await LoadExistingDataAsync();
        }

        public async Task LoadExistingDataAsync()
        {
            var entries = await _db.GetAllLocationsAsync();
            Locations.Clear();
            foreach (var e in entries) Locations.Add(e);
            PointCount = Locations.Count;
        }

        private async Task ToggleTrackingAsync()
        {
            if (IsTracking)
            {
                _locationService.StopTracking();
                IsTracking = false;
                StatusMessage = $"Stopped. {PointCount} points saved.";
            }
            else
            {
                StatusMessage = "Starting tracking…";
                await _locationService.StartTrackingAsync();
                IsTracking = true;
                StatusMessage = "Tracking — move around to build the heat map!";
            }
            OnPropertyChanged(nameof(TrackingButtonLabel));
        }

        private async Task ClearDataAsync()
        {
            await _db.ClearAllAsync();
            Locations.Clear();
            PointCount = 0;
            StatusMessage = "All data cleared.";
        }

        /// <summary>
        /// Manually adds a point near Cupertino CA with a tiny random offset.
        /// Creates a visible heat map cluster for emulator testing.
        /// </summary>
        private async Task AddManualPointAsync()
        {
            var rng = new Random();
            double lat = 37.3318 + (rng.NextDouble() - 0.5) * 0.002;
            double lon = -122.0312 + (rng.NextDouble() - 0.5) * 0.002;

            var entry = new LocationEntry
            {
                Latitude = lat,
                Longitude = lon,
                AccuracyMeters = 5.0,
                RecordedAtUtc = DateTime.UtcNow
            };

            await _db.SaveLocationAsync(entry);
            Locations.Add(entry);
            PointCount = Locations.Count;
            StatusMessage = $"Manual point {PointCount} added!";
        }

        private void OnLocationSaved(object? sender, LocationEntry entry)
        {
            Locations.Add(entry);
            PointCount = Locations.Count;
            StatusMessage =
                $"Tracking… {PointCount} pts | " +
                $"({entry.Latitude:F4}, {entry.Longitude:F4})";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));

        private bool SetField<T>(ref T field, T value,
            [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}