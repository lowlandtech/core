# LowlandTech Framework

LowlandTech is a modular .NET framework that provides dynamic functionality through plugins, configurable database providers, use cases for data operations, and entity triggers for handling database events.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
  - [Requirements](#requirements)
  - [Installation](#installation)
  - [Basic Setup](#basic-setup)
- [Plugin System](#plugin-system)
  - [Creating Plugins](#creating-plugins)
  - [Configuring Plugins](#configuring-plugins)
  - [Registering and Using Plugins](#registering-and-using-plugins)
- [Provider Configuration](#provider-configuration)
  - [Adding a Provider in a Plugin](#adding-a-provider-in-a-plugin)
  - [Using Migrations in a Plugin](#using-migrations-in-a-plugin)
- [Use Cases](#use-cases)
  - [Creating a Use Case](#creating-a-use-case)
  - [Executing a Use Case](#executing-a-use-case)
- [Entity Triggers](#entity-triggers)
  - [Implementing BeforeSave and AfterSave Triggers](#implementing-beforesave-and-aftersave-triggers)
- [Example Project Structure](#example-project-structure)
- [Contributing](#contributing)
- [License](#license)

## Overview

The LowlandTech framework allows modular development with plugins that have their own database contexts, migrations, and seeding mechanisms. It supports configurable data providers, use cases for database operations, and entity triggers for database lifecycle events, making it highly extensible and maintainable.

## Getting Started

### Requirements

- .NET 8 SDK
- [Entity Framework Core](https://docs.microsoft.com/ef/)
- [Lamar](https://jasperfx.github.io/lamar/) for Dependency Injection

### Installation

Install the core LowlandTech package:

```bash
dotnet add package LowlandTech.Core
```

### Basic Setup 
In your `Program.cs` file, initialize the framework with the required services and configurations:

```csharp
using LowlandTech.Core.Domain;
using LowlandTech.Plugin1;

var builder = WebApplication.CreateBuilder(args);

// Add provider configuration (optional)
builder.Services.Configure<ProviderOptions>(options =>
{
    options.Provider = Providers.Sqlite; // Choose the database provider
    options.UseSensitiveDataLogging = true;
    options.UseTriggers = true;
});

builder.Host.UseLamar((context, services) =>
{
    services.AddPlugin<Plugin1>(); // Register Plugin1, including DbContext, triggers, and use cases
});

var app = builder.Build();
app.UsePlugins(); // Configure all registered plugins

app.Run();
```

## Plugin System 

### Creating Plugins 
To create a plugin, implement the `IPlugin` interface or extend the base `Plugin` class. In the `Install` method, define the plugin’s dependencies, including any `DbContext` configurations and entity triggers.

```csharp
using LowlandTech.Core;
using Lamar;

[PluginId("YOUR_PLUGIN_GUID_HERE")]
public class Plugin1 : Plugin
{
    public Plugin1()
    {
        Id = new Guid("YOUR_PLUGIN_GUID_HERE");
    }

    public override void Install(ServiceRegistry services)
    {
        services.AddProvider<Plugin1Context>(); // Add DbContext with configured provider
        services.AddTransient<IBeforeSaveTrigger<Plugin1Entity>, Plugin1EntitySaving>();
        services.AddTransient<IAfterSaveTrigger<Plugin1Entity>, Plugin1EntitySaved>();
    }

    public override async void Configure(IContainer container)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider
            .GetRequiredService<IDbContextFactory<Plugin1Context>>();

        await app.UseMigration<Plugin1Context>(); // Apply migrations to the plugin's context
        await factory.Use<Plugin1UseCase, Plugin1Context>(); // Seed data using use cases
    }
}
```

### Configuring Plugins 

Plugins can be configured in two ways:
 
1. **Programmatic Registration** : As shown in `Program.cs` above, plugins can be registered using `AddPlugin<Plugin1>()`.
 
2. **Configuration File** : Alternatively, you can configure plugins using a `plugins.json` file. This allows dynamic loading and configuration of plugins based on runtime settings.
Example `plugins.json`Place a `plugins.json` file in the root of your project to define plugin options:

```json
{
  "PluginOptions": {
    "Plugins": [
      {
        "Name": "Plugin2",
        "IsActive": true
      },
      {
        "Name": "Plugin3",
        "IsActive": false
      }
    ]
  }
}
```

### Registering and Using Plugins 
To activate plugins, register them using `AddPlugin` in `Program.cs` or load them from `plugins.json` dynamically by calling `UsePlugins()` on the application instance.

```csharp
builder.Configuration.AddJsonFile("plugins.json", optional: false, reloadOnChange: true); // Add configuration file, reload must be set to true to use wordpress style plugins
builder.Host.UseLamar((context, services) =>
{
    services.AddPlugin<Plugin1>(); // Register Plugin1, including DbContext, triggers, and use cases
    servuces.AddPlugins(); // Loads plugins from configuration file, plugins 2 and 3
});
var app = builder.Build();
app.UsePlugins(); // Configure all active plugins from the configuration
```

## Provider Configuration 
The provider configuration defines the database provider (e.g., SQLite, PostgreSQL, SQL Server) and options for each plugin's `DbContext`.
### Adding a Provider in a Plugin 
Each plugin can define its own `DbContext` and associated provider settings. This is done using the `AddProvider` method in the `Install` method of the plugin.

```csharp
public override void Install(ServiceRegistry services)
{
    services.AddProvider<Plugin1Context>();
}
```

### Using Migrations in a Plugin 
Use the `UseMigration` extension method in the `Configure` method of a plugin to apply migrations to the associated `DbContext`.

```csharp
public override async void Configure(IContainer container)
{
    await app.UseMigration<Plugin1Context>(); // Apply migrations
}
```

## Use Cases 
Use cases in LowlandTech are actions that initialize or modify data in the `DbContext`, typically used for seeding or executing business logic.
### Creating a Use Case 
Implement `IUseCase` to define a use case for database operations:

```csharp
public class Plugin1UseCase : IUseCase
{
    private readonly Plugin1Context _context;

    public Plugin1UseCase(Plugin1Context context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        _context.Entities.Add(new Plugin1Entity { Name = "Sample Data" });
        await _context.SaveChangesAsync();
    }
}
```

### Executing a Use Case 
To execute a use case, call `Use<TUseCase>`:

```csharp
using var scope = app.Services.CreateScope();
var factory = scope.ServiceProvider
    .GetRequiredService<IDbContextFactory<Plugin1Context>>();

var context = await factory.CreateContextAsync();
await context.Use<Plugin1UseCase>();
```

## Entity Triggers 

Entity triggers allow you to perform actions before and after database operations, such as validation, auditing, or modifying entity properties.

### Implementing BeforeSave and AfterSave Triggers 
Define triggers by implementing `IBeforeSaveTrigger` or `IAfterSaveTrigger`. For example:

```csharp
public class Plugin1EntitySaving : IBeforeSaveTrigger<Plugin1Entity>
{
    public Task BeforeSave(ITriggerContext<Plugin1Entity> context, CancellationToken cancellationToken)
    {
        context.Entity.IsProcessed = true; // Set a flag to indicate processing
        return Task.CompletedTask;
    }
}

public class Plugin1EntitySaved : IAfterSaveTrigger<Plugin1Entity>
{
    public Task AfterSave(ITriggerContext<Plugin1Entity> context, CancellationToken cancellationToken)
    {
        context.Entity.IsSaved = true; // Set a flag to indicate saved state
        return Task.CompletedTask;
    }
}
```
Register the triggers in the `Install` method of the plugin:

```csharp
public override void Install(ServiceRegistry services)
{
    services.AddTransient<IBeforeSaveTrigger<Plugin1Entity>, Plugin1EntitySaving>();
    services.AddTransient<IAfterSaveTrigger<Plugin1Entity>, Plugin1EntitySaved>();
}
```

## Example Project Structure 

Below is an example structure for a LowlandTech project using plugins, providers, use cases, and triggers:


```css
├── src
│   ├── LowlandTech.App
│   │   ├── Program.cs
│   ├── LowlandTech.Core
│   │   ├── Domain
│   │   │   ├── ProviderExtensions.cs
│   │   │   └── UseCaseExtensions.cs
│   │   ├── Plugin.cs
│   ├── LowlandTech.Plugin1
│   │   ├── Plugin1.cs
│   │   ├── Plugin1Context.cs
│   │   ├── Entities
│   │   │   └── Plugin1Entity.cs
│   │   ├── Triggers
│   │   │   ├── Plugin1EntitySaving.cs
│   │   │   └── Plugin1EntitySaved.cs
│   │   └── UseCases
│   │       └── Plugin1UseCase.cs
└── plugins.json
```

## Contributing 

Contributions to LowlandTech are welcome! Please follow these steps:

1. Fork the repository.
 
2. Create a new feature branch (`git checkout -b feature/your-feature`).
 
3. Commit your changes (`git commit -m 'Add feature'`).
 
4. Push to the branch (`git push origin feature/your-feature`).

5. Open a pull request.

## License 
This project is licensed under the MIT License - see the [LICENSE](https://chatgpt.com/c/LICENSE)  file for details.

This `README.md` includes instructions for:
- Configuring plugins using both programmatic registration and `plugins.json`.
- Setting up multiple `DbContexts` with `AddProvider` and applying migrations within each plugin.
- Using triggers, providers, and use cases as self-contained functionality in each plugin, allowing for modularity and scalability in the LowlandTech framework.