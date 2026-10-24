namespace LowlandTech.Core.Sessions;

/// <summary>UI descriptor for a Kind: <paramref name="Key"/> is the lookup id (e.g. <c>"chat"</c>), <paramref name="Label"/> is the human-readable name, <paramref name="Icon"/> is an icon name resolved by the icon registry (not a URL).</summary>
public record KindDescriptor(string Key, string Label, string Icon);
