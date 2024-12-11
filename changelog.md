# Changelog
## 2024.12.1

- **Added** PropertyTypes to the `Property` class to allow for more complex property types.

## 2024-10-31

### Added

-   **Provider Configuration Enhancements**: Added support for multiple `DbContexts` per plugin, allowing each plugin to configure its own database provider.
-   **Enhanced Plugin Configuration**: Introduced support for configuring plugins through `plugins.json`, enabling dynamic loading and activation based on environment settings.
-   **Entity Triggers**: Implemented `BeforeSave` and `AfterSave` triggers for database entities, allowing for custom actions during entity lifecycle events.
-   **Use Case Execution**: Added `UseCaseExtensions` to simplify seeding and executing use cases in `DbContexts`.
-   **End-to-End Testing**: Improved test coverage with `WebApplicationFactory`, covering plugin initialization, provider configuration, migrations, and triggers.

### Fixed

-   **Migration Handling**: Ensured `UseMigration` correctly applies pending migrations for configured providers.

### Changed

-   **Modularization**: Refactored core plugin functionality to enhance modularity, allowing independent configuration of database contexts and providers within each plugin.