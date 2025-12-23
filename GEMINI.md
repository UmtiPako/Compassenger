# Compassenger

## Project Overview
Compassenger is a cross-platform mobile application built with **.NET MAUI**. It is designed to provide compass-based navigation and waypoint tracking. Users can save locations (waypoints) and use a digital compass to navigate towards them, with real-time distance and bearing updates.

## Tech Stack & Dependencies
*   **Framework:** .NET 8.0 MAUI (Targeting Android, iOS, MacCatalyst, Windows)
*   **Database:** SQLite via `Microsoft.EntityFrameworkCore.Sqlite`
*   **Mapping:** `Mapsui.Maui`
*   **Graphics:** SkiaSharp (via Mapsui dependencies)
*   **Math/Logic:** `MathNet.Filtering.Kalman` for sensor data smoothing (Kalman Filter).
*   **DI:** Standard Microsoft.Extensions.DependencyInjection.

## Architecture
The project follows a standard .NET MAUI structure with a focus on service-based logic and dependency injection.

### Key Directories
*   `\Data`: Contains the database context (`AppDbContext`) and repository layer (`Repo.cs`).
*   `\Models`: Entity definitions (e.g., `Waypoint.cs`).
*   `\Services`: Business logic and hardware abstraction (`CompassService.cs`, `LocationService.cs`).
*   `\Views`: UI pages and their code-behind logic (`CompassPage`, `MapView`, `LocationsView`).
*   `\Platforms`: Platform-specific configuration (AndroidManifest, Info.plist, etc.).

### Key Components
*   **MauiProgram.cs:** Application bootstrap. Configures DI container, fonts, and third-party libraries (Mapsui, SkiaSharp).
*   **CompassPage:** The main navigation interface. It handles permissions, hardware sensor subscriptions, and UI updates (arrow rotation, distance labels).
*   **SimpleKalmanFilter:** Used to smooth out noisy compass heading data for a stable UI experience.
*   **AppDbContext:** EF Core context configured to use a local SQLite database (`compassenger.db`) in the app's data directory.

## Data Flow
1.  **Hardware Sensors:** `CompassService` and `LocationService` provide raw data.
2.  **Processing:** `CompassPage` receives data, applies `SimpleKalmanFilter` to the heading, and calculates bearing/distance to the target `Waypoint`.
3.  **Persistence:** `Repo` (registered as Singleton) interacts with `AppDbContext` to save and retrieve user waypoints.

## Build & Run
**Prerequisites:**
*   .NET 8.0 SDK
*   .NET MAUI Workload (`dotnet workload install maui`)

**Commands:**
*   **Restore dependencies:**
    ```bash
    dotnet restore
    ```
*   **Build:**
    ```bash
    dotnet build -f net8.0-android  # or net8.0-ios, net8.0-windows10.0.19041.0
    ```
*   **Run (Android):**
    ```bash
    dotnet build -t:Run -f net8.0-android
    ```

## Development Conventions
*   **Dependency Injection:** Services and Pages are registered in `MauiProgram.cs`. Constructor injection is used for Pages.
*   **Async/Await:** Heavy usage of asynchronous patterns for I/O and sensor handling.
*   **Permissions:** Runtime permissions for Location are handled in the View's `OnAppearing` method.
