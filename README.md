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

## Overview

LocationHeatMap is a self-contained Android mobile application that addresses the problem of visualizing personal mobility patterns through spatial density estimation. The application continuously samples the device geographic position using the Android GPS hardware, persists each coordinate observation to a local SQLite relational database, and renders the accumulated dataset as an interactive heat map overlaid on a live OpenStreetMap tile layer. The visualization encodes visit frequency through a four-stop colour spectrum transitioning from blue through cyan, green, and yellow to red, where progressively warmer colours denote regions of higher spatial density and repeated presence.

The application was designed and implemented as a coursework submission for the CS Mobile Development module, demonstrating the integration of GPS sensor access, asynchronous data persistence, custom graphical rendering, and reactive UI architecture within the .NET MAUI cross-platform framework. All components are implemented using freely available and open-source libraries, requiring no commercial API subscriptions or third-party billing arrangements.

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

## Author

**Amer Shahbaz**
 June 2026

---


