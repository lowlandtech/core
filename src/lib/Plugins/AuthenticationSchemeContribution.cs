namespace LowlandTech.Core.Plugins;

/// <summary>
/// Base class a plugin derives from to contribute an ASP.NET Core authentication scheme.
/// SPEC2050 US12.
/// </summary>
public abstract class AuthenticationSchemeContribution
{
    public abstract string SchemeName { get; }
    public abstract string DisplayName { get; }
}
