# Project Overview: DMS (Device Management System)

This directory contains the source code for the Device Management System (DMS), a C#/.NET 8 application. The system is primarily composed of a WPF desktop application for the user interface, backed by a layered architecture including Core, Application, and Infrastructure projects. It also includes dedicated unit test projects for both the infrastructure and WPF layers.

A significant feature described in the README is an OPC UA service implementation, suggesting the system is designed for industrial device communication and management.

## Project Structure

The solution (`DMS.sln`) is organized into several key projects:

*   **`DMS.Core`**: Contains fundamental, reusable components and shared models. Likely includes utilities, common data transfer objects (DTOs), and core business logic abstractions. It references `Microsoft.Extensions.Logging.Abstractions`, `Newtonsoft.Json`, and `NLog`.
*   **`DMS.Application`**: Implements application-specific business logic. It depends on `DMS.Core` and uses libraries like `AutoMapper` for object mapping and `Microsoft.Extensions` for hosting and logging abstractions.
*   **`DMS.Infrastructure`**: Handles data access, external integrations, and technical implementations. It references `DMS.Core` and `DMS.Application`. Key dependencies include:
    *   `SqlSugarCore`: For database interaction (MySQL).
    *   `MQTTnet`: For MQTT protocol communication.
    *   `OPCFoundation.NetStandard.Opc.Ua`: For OPC UA communication.
    *   `S7netplus`: For Siemens S7 PLC communication.
    *   `NPOI`: For reading/writing Excel files.
    *   `AutoMapper.Extensions.Microsoft.DependencyInjection`: For dependency injection setup of AutoMapper.
*   **`DMS.WPF`**: The main WPF desktop application. It targets `net8.0-windows` and uses WPF-specific libraries like `CommunityToolkit.Mvvm`, `HandyControl` (UI toolkit), `Hardcodet.NotifyIcon.Wpf` (system tray icon), and `iNKORE.UI.WPF.Modern` (modern UI styling). It depends on `DMS.Application` and `DMS.Infrastructure`.
*   **`DMS.Infrastructure.UnitTests`**: Unit tests for the `DMS.Infrastructure` project. Uses `xunit`, `Moq`, `Bogus` (for fake data), and `Microsoft.NET.Test.Sdk`.
*   **`DMS.WPF.UnitTests`**: Unit tests for the `DMS.WPF` project. Also uses `xunit`, `Moq`, and `Bogus`.

## Building and Running

This is a .NET 8 solution. You will need the .NET 8 SDK and an IDE like Visual Studio or JetBrains Rider to build and run it.

**To Build:**
Use the standard .NET CLI command within the solution directory:
`dotnet build`

**To Run:**
The main executable is the WPF application (`DMS.WPF`). You can run it using:
`dotnet run --project DMS.WPF`

**To Test:**
Unit tests are included in `DMS.Infrastructure.UnitTests` and `DMS.WPF.UnitTests`.
Run all tests using:
`dotnet test`

## Development Conventions

*   **Language & Framework:** C# with .NET 8.
*   **Architecture:** Layered architecture (Core, Application, Infrastructure) with a WPF presentation layer.
*   **UI Pattern:** MVVM (Model-View-ViewModel), supported by `CommunityToolkit.Mvvm`.
*   **DI (Dependency Injection):** Microsoft.Extensions.DependencyInjection is used, configured likely in the `DMS.WPF` project.
*   **Logging:** NLog is used for logging, configured via `DMS.WPF\Configurations\nlog.config`.
*   **Mapping:** AutoMapper is used for object-to-object mapping.