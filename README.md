# LocationHeatMap

> A cross-platform mobile application for real-time GPS location tracking and kernel-density heat map visualization, implemented in C# using the .NET Multi-platform App UI (MAUI) framework with a SQLite persistence layer and OpenStreetMap tile integration.

<br/>

[![Platform](https://img.shields.io/badge/Platform-Android%20API%2034-3DDC84?style=flat-square&logo=android)](https://developer.android.com)
[![Framework](https://img.shields.io/badge/.NET-10.0%20MAUI-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/en-us/apps/maui)
[![Language](https://img.shields.io/badge/Language-C%23%2012-239120?style=flat-square&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Database](https://img.shields.io/badge/Database-SQLite-003B57?style=flat-square&logo=sqlite)](https://www.sqlite.org)
[![Map](https://img.shields.io/badge/Map-OpenStreetMap-7EBC6F?style=flat-square&logo=openstreetmap)](https://www.openstreetmap.org)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

<br/>

---

## Table of Contents

- [Overview](#overview)
- [Research Context and Motivation](#research-context-and-motivation)
- [System Architecture](#system-architecture)
- [Technical Stack](#technical-stack)
- [Project Structure](#project-structure)
- [Core Components](#core-components)
- [Heat Map Rendering Algorithm](#heat-map-rendering-algorithm)
- [Data Persistence Layer](#data-persistence-layer)
- [Prerequisites](#prerequisites)
- [Installation and Build](#installation-and-build)
- [Running on Emulator](#running-on-emulator)
- [Usage](#usage)
- [Coding Standards](#coding-standards)
- [Manifest Configuration](#manifest-configuration)
- [Known Limitations](#known-limitations)
- [Future Work](#future-work)
- [References](#references)
- [Author](#author)

---

## Overview

LocationHeatMap is a self-contained Android mobile application that addresses the problem of visualizing personal mobility patterns through spatial density estimation. The application continuously samples the device geographic position using the Android GPS hardware, persists each coordinate observation to a local SQLite relational database, and renders the accumulated dataset as an interactive heat map overlaid on a live OpenStreetMap tile layer. The visualization encodes visit frequency through a four-stop colour spectrum transitioning from blue through cyan, green, and yellow to red, where progressively warmer colours denote regions of higher spatial density and repeated presence.

The application was designed and implemented as a coursework submission for the CS Mobile Development module, demonstrating the integration of GPS sensor access, asynchronous data persistence, custom graphical rendering, and reactive UI architecture within the .NET MAUI cross-platform framework. All components are implemented using freely available and open-source libraries, requiring no commercial API subscriptions or third-party billing arrangements.

---

## Research Context and Motivation

The visualization of human mobility data through heat maps has established applications across urban planning, logistics optimization, behavioral research, and personal fitness analytics. Kernel density estimation (KDE), the statistical foundation underlying heat map visualization, provides a non-parametric approach to estimating the probability density function of a spatial point process from a finite set of discrete observations (Silverman, 1986). In mobile application contexts, the computational demands of exact KDE are typically approximated through radial gradient blending, which produces perceptually equivalent results at a fraction of the computational cost.

Existing commercial implementations of mobile heat map tracking rely on proprietary mapping SDKs — primarily Google Maps for Android and Apple Maps for iOS — which impose API key requirements associated with cloud billing accounts. This dependency creates a meaningful barrier for educational and research contexts. LocationHeatMap was designed specifically to eliminate this barrier by substituting OpenStreetMap tiles served through the Leaflet.js open-source JavaScript mapping library, achieving equivalent cartographic functionality under the Open Database License (ODbL).

---

## System Architecture

The application strictly adheres to the Model-View-ViewModel (MVVM) architectural pattern, enforced through namespace separation and unidirectional dependency constraints. The dependency graph is acyclic: the Models layer has no external dependencies; the Services layer depends only on Models; the Controls layer depends only on Models; the ViewModels layer depends on Models and Services but has zero references to any type in the Views or Controls namespaces; and the Views layer depends on ViewModels and Controls but contains no business logic.

```
┌─────────────────────────────────────────────────────┐
│                      Views                          │
│         MainView.xaml + MainView.xaml.cs            │
│   WebView (OSM) + GraphicsView (HeatMapOverlay)     │
└────────────────────┬────────────────────────────────┘
                     │ data binding / events
┌────────────────────▼────────────────────────────────┐
│                   ViewModels                        │
│                 MainViewModel                       │
│    INotifyPropertyChanged + ObservableCollection    │
└──────────┬──────────────────────────────────────────┘
           │ constructor injection (DI container)
┌──────────▼──────────┐    ┌──────────────────────────┐
│      Services       │    │        Controls           │
│  DatabaseService    │    │     HeatMapOverlay        │
│  LocationService    │    │    (IDrawable / MAUI      │
│  (GPS + SQLite I/O) │    │     GraphicsView)         │
└──────────┬──────────┘    └──────────────────────────┘
           │
┌──────────▼──────────┐
│       Models        │
│    LocationEntry    │
│   (SQLite ORM)      │
└─────────────────────┘
```

All service instantiation is delegated to the Microsoft.Extensions.DependencyInjection container configured in `MauiProgram.cs`. `DatabaseService` and `LocationService` are registered as singletons, ensuring a single shared database connection and a single GPS polling loop throughout the application lifetime. `MainView` is registered as transient, consistent with the recommendation that ContentPage instances not be retained across navigation events.

---

## Technical Stack

| Component | Technology | Version | Purpose |
|---|---|---|---|
| Language | C# | 12 | Primary implementation language |
| Runtime | .NET | 10.0 | Cross-platform execution environment |
| UI Framework | .NET MAUI | 10.0 | Native Android UI and platform APIs |
| Target Platform | Android | API 34 | Deployment target |
| ORM | sqlite-net-pcl | 1.9.172 | SQLite object-relational mapping |
| SQLite Bindings | SQLitePCLRaw.bundle_green | 2.1.10 | Native SQLite bindings for Android |
| Map Tiles | OpenStreetMap | — | Free, open-license street map data |
| Map Client | Leaflet.js | 1.9.4 | JavaScript map rendering in WebView |
| IDE | Visual Studio | 2026 v18.7.2 | Development environment |
| Emulator | Android Pixel 7 | API 34 x86_64 | Testing device |

---

## Project Structure

```
LocationHeatMap/
│
├── Controls/
│   └── HeatMapOverlay.cs          # Custom IDrawable heat map renderer
│
├── Models/
│   └── LocationEntry.cs           # SQLite-mapped GPS coordinate record
│
├── Platforms/
│   └── Android/
│       ├── AndroidManifest.xml    # Permissions and hardware declarations
│       ├── MainActivity.cs        # Android activity entry point
│       └── MainApplication.cs     # Android application class
│
├── Resources/
│   ├── AppIcon/                   # Application icon assets
│   ├── Fonts/                     # Bundled font files
│   ├── Raw/                       # Raw asset files
│   ├── Splash/                    # Splash screen assets
│   └── Styles/
│       ├── Colors.xaml            # Colour resource dictionary
│       └── Styles.xaml            # Control style definitions
│
├── Services/
│   ├── DatabaseService.cs         # SQLite CRUD operations
│   └── LocationService.cs         # GPS polling and event dispatch
│
├── ViewModels/
│   └── MainViewModel.cs           # MVVM binding hub and command handlers
│
├── Views/
│   ├── MainView.xaml              # UI layout — map, overlay, toolbar
│   └── MainView.xaml.cs           # Code-behind — canvas wiring, lifecycle
│
├── App.xaml                       # Application resources
├── App.xaml.cs                    # Application root and window creation
├── LocationHeatMap.csproj         # MSBuild project configuration
└── MauiProgram.cs                 # DI container registration and host config
```

---

## Core Components

### LocationEntry.cs

The data model class representing a single GPS coordinate observation. Decorated with sqlite-net-pcl attributes to enable automatic schema generation and typed ORM queries without manual SQL authorship.

```csharp
[Table("LocationEntries")]
public class LocationEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull] public double Latitude { get; set; }
    [NotNull] public double Longitude { get; set; }

    public double AccuracyMeters { get; set; }

    [NotNull] public DateTime RecordedAtUtc { get; set; }
}
```

Timestamps are stored in UTC to eliminate ambiguity during daylight saving transitions and to ensure consistent ordering across timezone boundaries.

---

### DatabaseService.cs

Manages the SQLite database lifecycle using a lazy initialization pattern. The `InitAsync` method is idempotent and safe to call from multiple code paths. All public methods are fully asynchronous, returning `Task` or `Task<T>` to prevent blocking the Android main thread and triggering Application Not Responding (ANR) conditions.

```csharp
public async Task InitAsync()
{
    if (_connection is not null) return;

    _connection = new SQLiteAsyncConnection(
        DbPath,
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create    |
        SQLiteOpenFlags.SharedCache);

    await _connection.CreateTableAsync<LocationEntry>();
}
```

A private `EnsureInitialised` guard method throws `InvalidOperationException` at any data access call site where `InitAsync` has not been completed, providing explicit diagnostic feedback during development if the initialization sequence is violated.

---

### LocationService.cs

Manages the GPS polling lifecycle through a `CancellationTokenSource`-controlled background loop executed via `Task.Run`. Location permission is requested at runtime using the MAUI `Permissions` API before polling begins. Each successful fix is persisted to SQLite and broadcast to subscribers via the `LocationSaved` event, marshalled to the UI thread using `MainThread.BeginInvokeOnMainThread` to ensure thread-safe data binding.

```csharp
private async Task PollLoopAsync(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        var location = await Geolocation.GetLocationAsync(request, token);

        if (location is not null)
        {
            var entry = new LocationEntry
            {
                Latitude       = location.Latitude,
                Longitude      = location.Longitude,
                AccuracyMeters = location.Accuracy ?? 0,
                RecordedAtUtc  = DateTime.UtcNow
            };

            await _db.SaveLocationAsync(entry);
            MainThread.BeginInvokeOnMainThread(
                () => LocationSaved?.Invoke(this, entry));
        }

        await Task.Delay(3000, token);
    }
}
```

---

### MainViewModel.cs

The MVVM binding hub. Exposes `ObservableCollection<LocationEntry>` for reactive UI updates, `ICommand` implementations for user interactions, and `INotifyPropertyChanged` for property-level change notification. The ViewModel has zero imports of any `Microsoft.Maui.Controls` type, maintaining strict MVVM discipline. A `SetField<T>` helper performs equality comparison before raising `PropertyChanged`, preventing redundant binding engine evaluations on high-frequency updates.

---

## Heat Map Rendering Algorithm

`HeatMapOverlay` implements `IDrawable` and is assigned to a `GraphicsView` positioned transparently above the `WebView` map. The rendering pipeline consists of two phases:

**Phase 1 — Coordinate Projection (UpdatePoints)**

GPS coordinates are projected to canvas pixel space using the equirectangular formula, which maps longitude linearly to the horizontal axis and latitude linearly to the inverted vertical axis. A 10% geographic padding is applied to the bounding box to prevent clipping of edge points. This projection is accurate to within acceptable tolerances for datasets spanning areas of up to approximately 50 km.

```csharp
float x = (float)((e.Longitude - bounds.MinLon) /
                   (bounds.MaxLon - bounds.MinLon)) * width;

float y = (float)(1.0 - (e.Latitude - bounds.MinLat) /
                         (bounds.MaxLat - bounds.MinLat)) * height;
```

**Phase 2 — Radial Gradient Rendering (Draw)**

Each projected point is rendered by `DrawBlob`, which draws eight concentric filled circles of geometrically decreasing radius. Each ring receives a colour interpolated from the four-stop heat map spectrum using linear interpolation between adjacent stops:

```
[0.00 – 0.25]  Blue    →  Cyan
[0.25 – 0.50]  Cyan    →  Green
[0.50 – 0.75]  Green   →  Yellow
[0.75 – 1.00]  Yellow  →  Red
```

Additive alpha blending of overlapping blobs from nearby or repeated coordinates produces the characteristic heat map density effect, where frequently visited locations accumulate warmer colours through visual superposition.

---

## Data Persistence Layer

Location data is stored in a single SQLite table named `LocationEntries` in the application private data directory at:

```
/data/data/com.amershahbaz.locationheatmap/files/locations.db3
```

The database file is not accessible to other applications on non-rooted Android devices, ensuring location data privacy without additional encryption. The database is initialized asynchronously on first page appearance rather than at application startup, avoiding blocking the main thread during the MAUI host initialization sequence.

Records accumulate indefinitely until the user explicitly invokes the Clear function, which executes `DeleteAllAsync<LocationEntry>()` and resets the in-memory `ObservableCollection` synchronously. No data is transmitted to any remote server at any point in the application lifecycle.

---

## Prerequisites

Before building the application, ensure the following components are installed and configured:

- **Visual Studio 2026** (version 18.7.2 or later) with the **.NET Multi-platform App UI development** workload
- **.NET 10 SDK** (included with the MAUI workload)
- **Android SDK** with API Level 34 platform package installed via the Android SDK Manager
- **Android Emulator** with a Pixel 7 virtual device (API 34, x86_64) or a physical Android device with USB debugging enabled
- **Hyper-V** enabled on Windows for hardware-accelerated emulation (required for x86_64 emulator images)

---

## Installation and Build

**Step 1** — Clone the repository:

```bash
git clone https://github.com/AmerShahbaz/LocationHeatMap.git
cd LocationHeatMap
```

**Step 2** — Open the solution in Visual Studio:

```
File → Open → Project/Solution → LocationHeatMap.slnx
```

**Step 3** — Restore NuGet packages (Visual Studio performs this automatically on solution open, or manually via):

```bash
dotnet restore
```

**Step 4** — Select the Android deployment target from the toolbar dropdown:

```
Pixel 7 - API 34.0 (Android 14.0 - API 34)
```

**Step 5** — Build the solution:

```
Build → Build Solution   (Ctrl+Shift+B)
```

Expected output:

```
========== Build: 1 succeeded, 0 failed, 0 skipped ==========
```

---

## Running on Emulator

**Step 1** — Start the Android emulator from the Android Device Manager:

```
Tools → Android → Android Device Manager → Pixel 7 ▶
```

**Step 2** — Wait for the Android home screen to appear (approximately 60 seconds on first boot).

**Step 3** — Press **F5** to deploy and launch the application. Visual Studio will build the APK, install it on the emulator, and launch it automatically.

**Step 4** — If the Visual Studio debugger pauses on internal Android runtime exceptions, press **F5 / Continue** to resume. These are framework-internal exceptions that do not affect application functionality.

**Step 5** — To simulate GPS movement on the emulator, open the Extended Controls panel via the `...` button on the emulator sidebar, navigate to **Location**, and enter coordinates manually under the **Single points** tab.

---

## Usage

| Control | Action |
|---|---|
| **▶ Start** | Requests location permission and begins GPS polling every 3 seconds |
| **⏹ Stop** | Cancels the polling loop and displays a summary of recorded points |
| **➕ Add Point** | Programmatically inserts a coordinate near Cupertino CA with a small random offset — useful for emulator heat map demonstration |
| **🗑 Clear** | Deletes all records from SQLite and resets the heat map canvas |
| **Map** | Pannable and zoomable via touch gestures (handled by Leaflet.js) |

The heat map repaints automatically on every new location event. Points accumulate in the SQLite database and are reloaded from storage on subsequent application launches, allowing heat map data to persist across sessions.

---

## Coding Standards

This project adheres to the following coding standards as specified in the assignment brief:

**Microsoft C# Coding Conventions** (Microsoft, 2024)
- PascalCase for all type names, method names, and public properties
- camelCase for local variables and method parameters
- Underscore prefix for private instance fields (`_connection`, `_isTracking`)
- XML documentation comments on all public APIs
- Nullable reference types enabled project-wide (`<Nullable>enable</Nullable>`)
- Async/await throughout all I/O operations; no synchronous blocking on the main thread
- `var` used where the declared type is immediately apparent from the initializer expression

**W3C JavaScript Best Practices** (W3C, 2024)
- Leaflet.js initialization code separated from HTML structure
- No inline JavaScript event handlers
- CDN-hosted library loaded with explicit version pinning

**Effective Dart** (Google, 2024)
- Dependency injection over service location
- Single responsibility principle enforced at the class level
- No static mutable state in business logic components

---

## Manifest Configuration

### AndroidManifest.xml

The Android manifest declares the following permissions and hardware features:

```xml
<!-- GPS-accuracy positioning -->
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />

<!-- Network-based positioning fallback -->
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />

<!-- Required for WebView to load OpenStreetMap tiles -->
<uses-permission android:name="android.permission.INTERNET" />

<!-- Declares GPS preference; required=false permits install on GPS-less devices -->
<uses-feature android:name="android.hardware.location.gps"
              android:required="false" />
```

Runtime permission for `ACCESS_FINE_LOCATION` is additionally requested via `Permissions.RequestAsync<Permissions.LocationWhenInUse>()` before the polling loop initiates, consistent with the Android 6.0+ runtime permission model.

---

## Known Limitations

| Limitation | Description | Mitigation |
|---|---|---|
| Equirectangular projection distortion | Positional accuracy degrades at high latitudes and for datasets spanning large geographic extents | Acceptable for city-scale movement datasets; Mercator projection recommended for larger extents |
| No background tracking | The GPS polling loop suspends when the application is backgrounded on Android 8.0+ due to the absence of a foreground service | User must keep the application in the foreground during tracking sessions |
| Approximate KDE | The radial blob rendering is a visual approximation of kernel density, not a statistically rigorous estimate | Suitable for visual exploration; quantitative density analysis requires server-side KDE computation |
| MAUI Maps substitution | OpenStreetMap via WebView is used in place of the specified MAUI Maps component due to the Google Maps API billing requirement | Functional outcome is equivalent; substitution is documented transparently |

---

## Future Work

The following enhancements are identified as directions for future development:

- **Background tracking service** — Implement an Android foreground service to continue GPS recording when the application is not in the foreground, using the MAUI platform project infrastructure to bind the MAUI activity to the service.
- **Session management** — Introduce a Session entity in the SQLite schema to group location records by tracking session, enabling comparison of movement patterns across days or weeks.
- **Mercator projection** — Replace the equirectangular projection in HeatMapOverlay with a Web Mercator projection to align the heat map overlay precisely with the Leaflet.js map tile coordinate system at all zoom levels.
- **Data export** — Implement GPX and GeoJSON export functionality to enable interoperability with GIS analysis tools such as QGIS and ArcGIS.
- **Configurable parameters** — Expose the polling interval, minimum movement threshold, blob radius, and colour scheme as user-configurable settings persisted to application preferences.
- **Statistical analysis** — Integrate dwell time estimation by grid cell using a Voronoi tessellation of the recorded points to produce quantitative density metrics alongside the visual heat map.

---

## References

Agafonkin, V. (2023). *Leaflet: An open-source JavaScript library for mobile-friendly interactive maps* (Version 1.9.4) [Computer software]. https://leafletjs.com

Google. (2023). *Request app permissions*. Android Developers Documentation. https://developer.android.com/training/permissions/requesting

Google. (2024). *Effective Dart: Style guide*. Dart Documentation. https://dart.dev/effective-dart/style

Haklay, M., & Weber, P. (2008). OpenStreetMap: User-generated street maps. *IEEE Pervasive Computing, 7*(4), 12–18. https://doi.org/10.1109/MPRV.2008.80

Hipp, D. R. (2020). *SQLite is a C-language library* (Version 3) [Computer software]. https://www.sqlite.org

Krueger, F. (2023). *sqlite-net-pcl* [Computer software]. GitHub. https://github.com/praeclarum/sqlite-net

Microsoft. (2023). *What is .NET MAUI?* .NET Documentation. https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui

Microsoft. (2024). *Common C# code conventions*. C# Programming Guide. https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

Silverman, B. W. (1986). *Density estimation for statistics and data analysis*. Chapman and Hall. https://doi.org/10.1007/978-1-4899-3324-9

W3C. (2024). *JavaScript best practices*. W3C Wiki. https://www.w3.org/wiki/JavaScript_best_practices

---

## Author

**Amer Shahbaz**
 June 2026

---


