using System.Text.Json;

namespace LowlandTech.Core.Contracts.Transitions;

/// <summary>The grid layout of a client surface.</summary>
public sealed record ClientLayout(
    int Cols,
    int Rows,
    IReadOnlyList<string> Columns,
    IReadOnlyList<string>? RowSizes = null);

/// <summary>The container of a client surface.</summary>
public sealed record ClientContainer(
    string Renderer,
    int ColStart,
    int ColEnd,
    int RowStart,
    int RowEnd,
    IReadOnlyList<ClientPageReference>? Pages = null,
    int? MobileOrder = null,
    bool MobileCollapsible = false,
    string? Title = null);

/// <summary>A page reference of a container. The renderer itself stays with the host.</summary>
public sealed record ClientPageReference(
    string Path,
    string Renderer,
    string Title,
    bool UserRequired = false,
    bool Anonymous = false);

/// <summary>
/// A component intent of a transition. The intent tree travels as data; the
/// component that renders it stays with the host.
/// </summary>
public sealed record ComponentIntent(
    string Key,
    string Label,
    string? Href = null,
    string? Client = null,
    string? Kind = null,
    IReadOnlyList<ComponentIntent>? Children = null,
    IReadOnlyList<JsonElement>? Flows = null);
