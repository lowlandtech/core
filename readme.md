# LowlandTech Plugin System

A flexible and extendable plugin system for .NET 8 applications using Lamar for dependency injection. This system allows you to dynamically load plugins at runtime, making it easy to add new features or extend existing functionality.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
  - [Requirements](#requirements)
  - [Installation](#installation)
  - [Basic Setup](#basic-setup)
- [Creating Plugins](#creating-plugins)
  - [Defining a Plugin](#defining-a-plugin)
  - [Registering a Plugin](#registering-a-plugin)
- [Using Plugins](#using-plugins)
- [Example Project Structure](#example-project-structure)
- [Contributing](#contributing)
- [License](#license)

## Overview

The LowlandTech Plugin System allows you to create and manage plugins that can be dynamically loaded into your .NET applications. It leverages Lamar for Dependency Injection and supports both runtime and compile-time plugin loading.

## Getting Started

### Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Lamar](https://jasperfx.github.io/lamar/) for dependency injection

### Installation

Install the core plugin system from the NuGet feed:

```bash
dotnet add package LowlandTech.Core
```

Make sure to configure the NuGet feed if you're using a private feed:


```Code kopiëren
dotnet nuget add source "https://nuget.lowlandtech.org/v3/index.json" --name "LowlandTech"
```

### Basic Setup 
In your `Program.cs`, set up the plugin system:

```Code kopiëren
using LowlandTech.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Lamar;

var builder = WebApplication.CreateBuilder(args);

// Load plugins configuration from plugins.json (located in the root of your project)
builder.Configuration.AddJsonFile("plugins.json", optional: false, reloadOnChange: true);

// Use Lamar for DI and add plugins
builder.Host.UseLamar((context, services) =>
{
    services.AddLogging();
    services.AddPlugins(); // Automatically registers plugins based on configuration
    services.AddPlugin<CustomPlugin>(); // Register a specific plugin manually
});

var app = builder.Build();
app.UsePlugins(); // Apply plugin configurations during app startup

app.Run();
```

### plugins.json Example 


```Code kopiëren
{
  "PluginOptions": {
    "Plugins": [
      {
        "Name": "PluginA",
        "IsActive": true
      },
      {
        "Name": "PluginB",
        "IsActive": true
      }
    ]
  }
}
```
This configuration allows the system to load and activate `PluginA` and `PluginB` at runtime.
## Creating Plugins 

### Defining a Plugin 
To create a plugin, implement the `IPlugin` interface or use the abstract base class Plugin:

```Code kopiëren
using LowlandTech.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

public class CustomPlugin : IPlugin
{
    public string Name { get; set; } = "CustomPlugin";
    public bool IsActive { get; set; } = true;

    public void Install(ServiceRegistry services)
    {
        // Register services specific to this plugin
        services.AddSingleton<MyCustomService>();
    }

    public void Configure(WebApplication app)
    {
        // Configure middleware or other application settings specific to this plugin
        app.MapGet("/custom", () => "Hello from Custom Plugin!");
    }
}
```

### Registering a Plugin 
You can register a plugin either programmatically or through the `plugins.json` configuration. 
- **Programmatically**  in `Program.cs`:

```Code kopiëren
services.AddPlugin<CustomPlugin>();
```
 
- **Using `plugins.json`** :Add the plugin configuration to `plugins.json`:

```Code kopiëren
{
  "PluginOptions": {
    "Plugins": [
      {
        "Name": "CustomPlugin",
        "IsActive": true
      }
    ]
  }
}
```

## Using Plugins 

Once your plugins are registered and configured, you can access them in your application:
 
- **Accessing Plugins** :
Use Dependency Injection to access plugin services:


```Code kopiëren
var customService = app.Services.GetRequiredService<MyCustomService>();
```
 
- **Consuming Plugin Endpoints** :If your plugin registers routes, you can access them directly via the URL, e.g., `https://localhost:5001/custom`.
 
- **Getting All Plugins** :If you have a need to list all active plugins, you can use the `/plugins` endpoint if you have it set up:

```Code kopiëren
app.MapGet("/plugins", () =>
{
    var scope = app.Services.CreateScope();
    var plugins = scope.ServiceProvider.GetServices<IPlugin>();
    return plugins.Select(p => new { p.Name, p.IsActive });
}).WithName("GetPlugins");
```

## Example Project Structure 

Here's a basic structure for a project using the plugin system:


```Code kopiëren
├── src
│   ├── MyApp
│   │   ├── Program.cs
│   │   ├── plugins.json
│   │   └── Startup.cs
│   ├── MyApp.Plugins
│   │   ├── PluginA.cs
│   │   ├── PluginB.cs
│   │   └── CustomPlugin.cs
│   └── MyApp.Tests
│       └── PluginTests.cs
└── README.md
```
 
- `MyApp`: Main application project.
 
- `MyApp.Plugins`: Contains plugins like `PluginA`, `PluginB`, and `CustomPlugin`.
 
- `MyApp.Tests`: Contains integration tests for plugins.

## Contributing 

Contributions are welcome! Please follow these steps:

1. Fork the repository.
 
2. Create a feature branch (`git checkout -b feature/YourFeature`).
 
3. Commit your changes (`git commit -m 'Add your feature'`).
 
4. Push to the branch (`git push origin feature/YourFeature`).

5. Open a pull request.

## License 
This project is licensed under the MIT License - see the [LICENSE]()  file for details.

### Summary of the `README.md`

- **Overview**: Brief introduction to the plugin system.
- **Getting Started**: Provides instructions on setting up the project and installing the plugin system.
- **Creating Plugins**: Walks through creating a plugin by implementing the `IPlugin` interface.
- **Using Plugins**: Details how to register and use plugins within the application.
- **Example Project Structure**: Gives an example layout for a project using the plugin system.
- **Contributing**: Encourages contributions and outlines how to do so.
- **License**: Mentions the license under which the project is distributed.

This `README.md` should serve as a comprehensive guide for new users to understand how to integrate and use the plugin system in their .NET 8 projects. Adjust the details according to your project as needed.