namespace LowlandTech.Core.Plugins.InstallModes;

/// <summary>
/// One ordered unit of work within an <see cref="InstallPlan"/>.
/// SPEC2050 US20.
/// </summary>
public sealed record InstallStep(string Name, string Description);
