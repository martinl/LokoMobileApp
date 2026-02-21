# Loko MAUI App - Architecture Diagram

## Overview

**Loko** is a .NET MAUI cross-platform mobile application (Android & iOS) for BLE (Bluetooth Low Energy) device tracking with real-time GPS location mapping. It connects to BLE-enabled hardware devices, receives location telemetry, and visualizes device positions on both Google Maps and OpenStreetMap with offline map support.

**Target Frameworks:** `net10.0-ios`, `net10.0-android`

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          LOKO MAUI APPLICATION                         │
│                     (.NET MAUI / C# / MVVM Pattern)                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                        PRESENTATION LAYER                         │  │
│  │                                                                   │  │
│  │  ┌─────────────┐  ┌──────────────┐  ┌────────────────────────┐   │  │
│  │  │  MapPage     │  │ OSMPage      │  │  ConnectionPage        │   │  │
│  │  │  (Google     │  │ (OpenStreet  │  │  (Settings / BLE       │   │  │
│  │  │   Maps)      │  │  Map via     │  │   Connection /         │   │  │
│  │  │  .xaml/.cs   │  │  Mapsui)     │  │   Map Config)          │   │  │
│  │  └──────┬───────┘  └──────┬───────┘  └──────────┬─────────────┘   │  │
│  │         │                 │                     │                  │  │
│  │  ┌──────┴─────┐  ┌───────┴──────┐  ┌───────────┴──────────┐      │  │
│  │  │ ArcivePage │  │ DownloadMap  │  │ MyOfflineMapsPage    │      │  │
│  │  │ (History   │  │ Page         │  │ (Manage Downloaded   │      │  │
│  │  │  Playback) │  │ (Tile DL)   │  │  Map Areas)          │      │  │
│  │  └────────────┘  └─────────────┘  └──────────────────────┘      │  │
│  │                                                                   │  │
│  │  Custom Controls:                                                 │  │
│  │  ┌──────────────┐  ┌────────────────────┐  ┌─────────────────┐   │  │
│  │  │ ExtendedMap  │  │ ExtendedMapsuiView │  │ CustomToggle    │   │  │
│  │  │ (Google Maps │  │ (Mapsui/SkiaSharp  │  │ (XAML Control)  │   │  │
│  │  │  + Polylines)│  │  Map Renderer)     │  │                 │   │  │
│  │  └──────────────┘  └────────────────────┘  └─────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                    │                                    │
│                                    ▼                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                        VIEWMODEL LAYER                            │  │
│  │                  (CommunityToolkit.Mvvm / MVVM)                   │  │
│  │                                                                   │  │
│  │  ┌──────────────────┐                                             │  │
│  │  │  BaseViewModel   │◄──── ObservableObject (MVVM Toolkit)        │  │
│  │  │  - Title         │      + RelayCommand, ObservableProperty     │  │
│  │  │  - GoBack()      │                                             │  │
│  │  │  - Initialize()  │                                             │  │
│  │  └────────┬─────────┘                                             │  │
│  │           │                                                       │  │
│  │  ┌────────┴──────────────────────────────────────────────────┐    │  │
│  │  │                          │                                │    │  │
│  │  ▼                          ▼                                ▼    │  │
│  │  ┌─────────────┐  ┌────────────────────┐  ┌──────────────────┐   │  │
│  │  │ MapViewModel│  │ConnectionPage      │  │ArchivePageView  │   │  │
│  │  │             │  │ViewModel           │  │ Model            │   │  │
│  │  │ - Places    │  │                    │  │                  │   │  │
│  │  │ - PlacesMui │  │ - Settings[]       │  │ - MapItemsSource│   │  │
│  │  │ - Devices   │  │ - BLEDevices[]     │  │ - DevicesList   │   │  │
│  │  │ - UserLoc   │  │ - BatteryLevel     │  │ - Title (date)  │   │  │
│  │  │ - MapType   │  │ - IsScanning       │  │                  │   │  │
│  │  └─────────────┘  └────────────────────┘  └──────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                    │                                    │
│                                    ▼                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                         SERVICE LAYER                             │  │
│  │               (Dependency Injection / Singletons)                 │  │
│  │                                                                   │  │
│  │  ┌──────────────────────────────────────────────────────────┐     │  │
│  │  │              Interfaces                                  │     │  │
│  │  │  ┌──────────────────────┐  ┌─────────────────────────┐  │     │  │
│  │  │  │ IBLEConnectionService│  │ INavigationService      │  │     │  │
│  │  │  │ - ScanDevices()      │  │ - NavigateToMapPage()   │  │     │  │
│  │  │  │ - ConnectToDevice()  │  │ - NavigateToArcivePage()│  │     │  │
│  │  │  │ - DisconnectDevice() │  │ - NavigateToConnection()│  │     │  │
│  │  │  │ - DeviceDiscovered   │  │ - ChangeRootPage()      │  │     │  │
│  │  │  │ - BatteryLevel       │  │ - NavigateBack()        │  │     │  │
│  │  │  └──────────┬───────────┘  └────────────┬────────────┘  │     │  │
│  │  │  ┌──────────┘                           │               │     │  │
│  │  │  │  ┌───────────────────┐               │               │     │  │
│  │  │  │  │IPermissionService │               │               │     │  │
│  │  │  │  │ - RequestBLE()    │               │               │     │  │
│  │  │  │  │ - RequestLoc()    │               │               │     │  │
│  │  │  │  └───────────────────┘               │               │     │  │
│  │  └──┼──────────────────────────────────────┼───────────────┘     │  │
│  │     │          Implementations             │                     │  │
│  │     ▼                                      ▼                     │  │
│  │  ┌──────────────────────┐  ┌───────────────────────────┐         │  │
│  │  │ BLEConnectionService │  │ NavigationService         │         │  │
│  │  │ (Plugin.BLE)         │  │ (Shell-less Navigation)   │         │  │
│  │  │                      │  └───────────────────────────┘         │  │
│  │  │ - Adapter.Scan()     │                                        │  │
│  │  │ - Connect/Disconnect │  ┌───────────────────────────┐         │  │
│  │  │ - Read BLE Char.     │  │ LocationService           │         │  │
│  │  │ - Parse CSV telemetry│  │ (Geolocation Foreground)  │         │  │
│  │  │   (id,name,lat,lon,  │  │ - Start/Stop Listening    │         │  │
│  │  │    battery)           │  │ - LocationChanged event   │         │  │
│  │  └──────────────────────┘  └───────────────────────────┘         │  │
│  │                                                                   │  │
│  │  ┌────────────────────────────────────────────────────────────┐   │  │
│  │  │                  MAP / TILE SERVICES                        │   │  │
│  │  │                                                            │   │  │
│  │  │  ┌──────────────────┐  ┌──────────────────────────────┐   │   │  │
│  │  │  │ TileDownloader   │  │ CustomTileSource             │   │   │  │
│  │  │  │ (BruTile/HTTP)   │  │ (ITileSource - online/       │   │   │  │
│  │  │  │ - DownloadArea() │  │  offline fallback)            │   │   │  │
│  │  │  │ - SaveTile()     │  │ - GetTileAsync()             │   │   │  │
│  │  │  │ - CalcTiles()    │  │ - GetCachedTileAsync()       │   │   │  │
│  │  │  └──────────────────┘  └──────────────────────────────┘   │   │  │
│  │  │                                                            │   │  │
│  │  │  ┌──────────────────────────────┐                          │   │  │
│  │  │  │ DownloadedAreaManager        │                          │   │  │
│  │  │  │ (SQLite - AreasDB.db3)       │                          │   │  │
│  │  │  │ - SaveArea / DeleteArea      │                          │   │  │
│  │  │  │ - GetAreas / DeleteTiles     │                          │   │  │
│  │  │  └──────────────────────────────┘                          │   │  │
│  │  └────────────────────────────────────────────────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                    │                                    │
│                                    ▼                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                          DATA LAYER                               │  │
│  │                                                                   │  │
│  │  ┌──────────────────────────────────────────────────┐             │  │
│  │  │ DeviceDetailsDB (SQLite - DeviceDetailsSQLite.db3)│            │  │
│  │  │ - SaveRecordToBase(BLEDeviceDetails)              │            │  │
│  │  │ - GetRecordsByDeviceId(int)                       │            │  │
│  │  │ - GetRecordsByDate(string)                        │            │  │
│  │  │ - GetDatesFromRecords()                           │            │  │
│  │  │ - GetDeviceModels()                               │            │  │
│  │  │ - DeleteRecordsByDate()                           │            │  │
│  │  └──────────────────────────────────────────────────┘             │  │
│  │                                                                   │  │
│  │  ┌────────────────────────────────────────────────────┐           │  │
│  │  │ AppPreferences (MAUI Preferences API)              │           │  │
│  │  │ - SelectedMapType (Google / OSM)                   │           │  │
│  │  └────────────────────────────────────────────────────┘           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                    │                                    │
│                                    ▼                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                          MODEL LAYER                              │  │
│  │                                                                   │  │
│  │  ┌───────────────┐  ┌──────────────────┐  ┌──────────────────┐   │  │
│  │  │ BLEDevice     │  │ BLEDeviceDetails │  │ DBDeviceDetails  │   │  │
│  │  │ - Device(IDevice)│ - DeviceId       │  │ (SQLite Entity)  │   │  │
│  │  │ - Characteristic│ - Lat/Lon        │  │ - Id, DeviceId   │   │  │
│  │  │ - Locations[] │  │ - BatteryLevel   │  │ - Lat/Lon        │   │  │
│  │  │ - IsConnected │  │ - DateTime       │  │ - BatteryLevel   │   │  │
│  │  │ - Details     │  │ - IsUserLocation │  │ - DateTime       │   │  │
│  │  └───────────────┘  └──────────────────┘  └──────────────────┘   │  │
│  │                                                                   │  │
│  │  ┌───────────────┐  ┌──────────────────┐  ┌──────────────────┐   │  │
│  │  │ DeviceModel   │  │ SettingsOptions  │  │ DownloadedArea   │   │  │
│  │  │ - Label       │  │ - Label          │  │ - Name           │   │  │
│  │  │ - IsSelected  │  │ - Contents[]     │  │ - Min/Max Lat/Lon│   │  │
│  │  └───────────────┘  │ - IsExpanded     │  │ - Min/Max Zoom   │   │  │
│  │                      │ - SettingsType   │  │ - DownloadSize   │   │  │
│  │                      └──────────────────┘  └──────────────────┘   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                    FRAMEWORK / BASE CLASSES                              │
│                                                                         │
│  ┌──────────────────────┐  ┌──────────────────────────────────────┐    │
│  │ BaseContentPage<T>   │  │ BaseViewModel                       │    │
│  │ - Auto-binds VM      │  │ - ObservableObject (MVVM Toolkit)   │    │
│  │ - OnAppearing →      │  │ - InitializeAsync / UnInitializeAsync│   │
│  │   VM.InitializeAsync │  │ - GoBackCommand (RelayCommand)      │    │
│  │ - OnDisappearing →   │  │ - Dispose pattern                   │    │
│  │   VM.UnInitializeAsync│ └──────────────────────────────────────┘    │
│  └──────────────────────┘                                              │
│                                                                         │
│  ┌──────────────────────┐  ┌──────────────────────────────────────┐    │
│  │ BindableBase         │  │ NetworkHelper                        │    │
│  │ (INotifyPropChanged) │  │ - IsNetworkAvailable()              │    │
│  └──────────────────────┘  └──────────────────────────────────────┘    │
├─────────────────────────────────────────────────────────────────────────┤
│                   DEPENDENCY INJECTION (MauiProgram.cs)                  │
│                                                                         │
│  Services (Singleton):                                                  │
│    IBLEConnectionService → BLEConnectionService                         │
│    INavigationService    → NavigationService                            │
│    IPermissionService    → PermissionService                            │
│                                                                         │
│  ViewModels (Singleton):                                                │
│    MapViewModel, ArchivePageViewModel, ConnectionPageViewModel          │
│                                                                         │
│  Views (Transient/Singleton):                                           │
│    MapPage, OSMPage, ConnectionPage (Transient)                         │
│    ArcivePage, DownloadMapPage (Singleton)                              │
│    MyOfflineMapsPage (Transient)                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                        EXTERNAL DEPENDENCIES                            │
│                                                                         │
│  ┌──────────────────┐ ┌────────────────┐ ┌──────────────────────────┐  │
│  │ Plugin.BLE 3.1   │ │ Mapsui.Maui    │ │ Microsoft.Maui.Maps      │  │
│  │ (BLE Scanning,   │ │ 4.1.7          │ │ 8.0.60 (Google Maps)     │  │
│  │  Connect, GATT)  │ │ (OSM Renderer) │ │                          │  │
│  └──────────────────┘ └────────────────┘ └──────────────────────────┘  │
│  ┌──────────────────┐ ┌────────────────┐ ┌──────────────────────────┐  │
│  │ sqlite-net-pcl   │ │ SkiaSharp      │ │ CommunityToolkit.Maui    │  │
│  │ 1.9.172          │ │ (2D Graphics)  │ │ 9.0.1 + MVVM 8.2.2      │  │
│  │ (Local DB)       │ │                │ │                          │  │
│  └──────────────────┘ └────────────────┘ └──────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ BruTile (Tile Schema, Tile Source, OpenStreetMap Tile Fetching)  │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow Diagrams

### 1. BLE Device Tracking Flow

```
┌──────────────┐     BLE Scan      ┌────────────────────┐
│  BLE Hardware │ ◄──────────────── │ BLEConnectionService│
│  Device       │                   │ (Plugin.BLE)        │
│  (GPS Tracker)│ ────────────────► │                     │
│               │  Notify (CSV):    │ Parse CSV:          │
│               │  id,name,lat,     │ id, lat, lon,       │
│               │  lon,battery      │ battery              │
└──────────────┘                   └─────────┬────────────┘
                                             │
                                             │ DeviceDiscovered event
                                             │ BLEDevice.Details updated
                                             ▼
                               ┌─────────────────────────┐
                               │  MapViewModel /          │
                               │  ConnectionPageViewModel │
                               │                          │
                               │  OnDevicePropertyChanged │
                               └──────────┬──────────────┘
                                          │
                          ┌───────────────┼───────────────┐
                          │               │               │
                          ▼               ▼               ▼
                  ┌──────────────┐ ┌────────────┐ ┌──────────────┐
                  │ Places[]     │ │PlacesMui[] │ │DeviceDetailsDB│
                  │ (Google Pins)│ │(OSM Pins)  │ │(SQLite Save)  │
                  └──────┬───────┘ └─────┬──────┘ └──────────────┘
                         │               │
                         ▼               ▼
                  ┌──────────────┐ ┌────────────────────┐
                  │ ExtendedMap  │ │ ExtendedMapsuiView │
                  │ (Pins +     │ │ (Pins +             │
                  │  Polylines) │ │  SkiaSharp render)  │
                  └──────────────┘ └────────────────────┘
```

### 2. Map Provider Switching Flow

```
┌────────────────────┐   Toggle "OSM"   ┌──────────────────────┐
│  ConnectionPage    │ ──────────────► │  App.UseOSM = true    │
│  (Settings)        │                 │  AppPreferences.Set() │
└────────────────────┘                 └──────────┬────────────┘
                                                  │
                                                  │ OnUseOSMChanged event
                                                  ▼
                                       ┌──────────────────────┐
                                       │  App.SetInitialPage() │
                                       │                       │
                                  ┌────┴────┐           ┌─────┴─────┐
                                  │ OSMPage │           │  MapPage  │
                                  │ (Mapsui │           │ (Google   │
                                  │  tiles) │           │  Maps)    │
                                  └─────────┘           └───────────┘
```

### 3. Offline Map Tile Flow

```
┌─────────────────┐   Select Area   ┌────────────────┐
│ DownloadMapPage │ ──────────────► │ TileDownloader  │
│                 │   + Zoom Range  │                 │
└─────────────────┘                 │ - HTTP fetch    │
                                    │   from OSM      │
                                    │ - Save to       │
                                    │   FileSystem    │
                                    └────────┬────────┘
                                             │
                                             ▼
                                    ┌─────────────────────┐
                                    │ DownloadedAreaManager│
                                    │ (AreasDB.db3)        │
                                    │ - Store area metadata│
                                    └─────────────────────┘
                                             │
                                             ▼
┌─────────────────┐                ┌─────────────────────┐
│ CustomTileSource│ ◄───────────── │ MapTileCache/       │
│                 │  Offline       │ {name}/{z}/{x}/{y}  │
│ Online? → HTTP  │  fallback     │ .png                 │
│ Offline? → Cache│                └─────────────────────┘
└─────────────────┘
```

---

## Navigation Structure

```
                        ┌──────────────┐
                        │   App.xaml   │
                        │ (Entry Point)│
                        └──────┬───────┘
                               │
                    ┌──────────┴──────────┐
                    │                     │
             UseOSM = false         UseOSM = true
                    │                     │
                    ▼                     ▼
            ┌──────────────┐      ┌──────────────┐
            │   MapPage    │      │   OSMPage    │
            │ (Google Maps)│      │ (Mapsui/OSM) │
            └──────┬───────┘      └──────┬───────┘
                   │                     │
                   └──────────┬──────────┘
                              │ Settings button
                              ▼
                    ┌──────────────────┐
                    │ ConnectionPage   │
                    │ (Settings Hub)   │
                    └────────┬─────────┘
                             │
              ┌──────────────┼────────────────┐
              │              │                │
              ▼              ▼                ▼
     ┌──────────────┐ ┌───────────────┐ ┌──────────────────┐
     │  ArcivePage  │ │ DownloadMap   │ │ MyOfflineMapsPage│
     │  (History)   │ │ Page          │ │ (Manage Areas)   │
     └──────────────┘ └───────────────┘ └──────────────────┘
```

---

## Key Architectural Decisions

| Decision | Choice | Rationale |
|---|---|---|
| **Framework** | .NET MAUI | Cross-platform (Android + iOS) from single codebase |
| **Pattern** | MVVM | Clean separation of UI and business logic |
| **MVVM Toolkit** | CommunityToolkit.Mvvm | Source generators for ObservableProperty, RelayCommand |
| **BLE** | Plugin.BLE | Mature cross-platform BLE abstraction |
| **Google Maps** | Microsoft.Maui.Controls.Maps | Native map experience with pins + polylines |
| **OSM Maps** | Mapsui.Maui + SkiaSharp | Offline-capable OpenStreetMap rendering |
| **Tile Engine** | BruTile | Tile schema, HTTP tile fetching, caching |
| **Local DB** | sqlite-net-pcl | Lightweight async SQLite for device telemetry |
| **DI** | Microsoft.Extensions.DI | Built-in MAUI service container |
| **Navigation** | NavigationPage (no Shell) | Simple stack-based push/pop navigation |
| **Preferences** | MAUI Preferences API | Key-value storage for user settings |

---

## Project File Structure

```
LokoMaui/
├── App.xaml / App.xaml.cs          # App entry, map type switching
├── MauiProgram.cs                  # DI registration, MAUI builder config
├── global.json                     # .NET SDK version pinning
│
├── Framework/
│   ├── BaseViewModel.cs            # Abstract VM with lifecycle hooks
│   └── BaseContentPage.cs          # Generic page with auto VM binding
│
├── Models/
│   ├── BLEDevice.cs                # BLE device with connection state
│   ├── BLEDeviceDetails.cs         # Location telemetry data point
│   ├── DBDeviceDetails.cs          # SQLite entity for persistence
│   ├── DeviceModel.cs              # UI model for device selection
│   ├── SettingsOptions.cs          # Settings menu structure model
│   └── CustomCalloutStyle.cs       # Map callout styling
│
├── ViewModels/
│   ├── MapPageViewModel.cs         # Main map logic, BLE data → pins
│   ├── ConnectionPageViewModel.cs  # BLE scanning, settings, archive
│   └── ArchivePageViewModel.cs     # Historical data viewing
│
├── Views/
│   ├── MapPage.xaml/.cs            # Google Maps view
│   ├── OSMPage.xaml/.cs            # OpenStreetMap (Mapsui) view
│   ├── ConnectionPage.xaml/.cs     # Settings & BLE connection page
│   ├── ArcivePage.xaml/.cs         # Archive/history map view
│   ├── DownloadMapPage.xaml/.cs    # Offline map area download
│   └── MyOfflineMapsPage.xaml/.cs  # Manage downloaded map areas
│
├── Services/
│   ├── Interfaces/
│   │   ├── IBLEConnectionService.cs
│   │   ├── INavigationService.cs
│   │   └── IPermissionService.cs
│   ├── Implementations/
│   │   ├── BLEConnectionService.cs # Plugin.BLE adapter, GATT comms
│   │   ├── NavigationService.cs    # Stack-based page navigation
│   │   ├── LocationService.cs      # Foreground geolocation tracking
│   │   ├── DeviceDetailsDB.cs      # SQLite CRUD for telemetry
│   │   └── TileDownloader.cs       # OSM tile download engine
│   └── DownloadAreaManager.cs      # SQLite CRUD for offline map areas
│
├── Controls/
│   ├── CustomToggle.xaml/.cs       # Custom toggle switch control
│   ├── ExpanderDataTemplateSelector.cs
│   └── Renderers/
│       ├── ExtendedMap.cs          # Google Maps with polyline support
│       ├── ExtendedMapsuiView.cs   # Mapsui map with custom pins
│       └── ExtendedPin.cs          # Pin model with device details
│
├── Helpers/
│   ├── AppPreferences.cs           # MAUI Preferences wrapper
│   ├── BindableBase.cs             # INotifyPropertyChanged base
│   ├── CustomTileSource.cs         # Online/offline tile source
│   ├── NetworkHelper.cs            # Connectivity check
│   └── Extensions/
│       ├── RuntimePermission.cs    # Permission request helpers
│       └── ViewExtensions.cs       # UI extension methods
│
├── Platforms/
│   ├── Android/                    # Android-specific handlers
│   └── iOS/                        # iOS-specific assets
│
└── Resources/
    ├── AppIcon/                    # App icons
    ├── Fonts/                      # Montserrat + OpenSans fonts
    ├── Images/                     # Pin icons, logos
    ├── Raw/                        # Raw assets
    └── Splash/                     # Splash screen
```
