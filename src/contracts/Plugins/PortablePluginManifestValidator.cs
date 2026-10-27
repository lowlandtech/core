using System.Text.Json;

namespace LowlandTech.Core.Contracts.Plugins;

/// <summary>
/// A refusal of a plugin manifest. The refusal is named and versioned: it
/// carries the schema version the manifest declares and the members and
/// declarations that version does not accept, so the consumer of the refusal
/// knows which version to move to instead of guessing why it was rejected.
/// </summary>
public sealed record PortablePluginManifestRefusal(
    int SchemaVersion,
    IReadOnlyList<string> RefusedMembers,
    IReadOnlyList<string> RefusedDeclarations,
    string Message);

/// <summary>The outcome of reading a plugin manifest.</summary>
public sealed record PortablePluginManifestReadResult(
    PortablePluginManifestDeclaration? Manifest,
    int SchemaVersion,
    IReadOnlyList<string> RefusedMembers,
    IReadOnlyList<string> RefusedDeclarations,
    string Message)
{
    /// <summary>Whether the manifest was accepted.</summary>
    public bool IsAccepted => Manifest is not null;

    /// <summary>The refusal of the manifest, when it was refused.</summary>
    public PortablePluginManifestRefusal Refusal => new(SchemaVersion, RefusedMembers, RefusedDeclarations, Message);
}

/// <summary>
/// Reads a plugin manifest into the portable declaration.
/// </summary>
/// <remarks>
/// <para>
/// The reader never resolves a name it reads. A contribution key, a component
/// key and a route component stay strings, so a manifest that names a type
/// that does not exist is still read, and an untrusted document can never make
/// the contract load a type.
/// </para>
/// <para>
/// The reader accepts the schema versions the published hosts use and refuses
/// every other shape with the name of the member or the declaration that
/// carries the violation. The rules mirror the rules of the published Plugins
/// inspector, so a manifest the inspector accepts is accepted here and a
/// manifest the inspector refuses is refused here with the same name.
/// </para>
/// </remarks>
public static class PortablePluginManifestValidator
{
    /// <summary>The schema version of the first manifest.</summary>
    public const int ManifestSchemaVersion = 1;

    /// <summary>The schema version that accepts an installation unit.</summary>
    public const int MultiContributionManifestSchemaVersion = 2;

    /// <summary>The schema version that accepts client assets.</summary>
    public const int AssetManifestSchemaVersion = 3;

    /// <summary>The schema version that accepts a versioned target and definition files.</summary>
    public const int VersionedManifestSchemaVersion = MultiContributionManifestSchemaVersion;

    /// <summary>Reads a plugin manifest document.</summary>
    public static PortablePluginManifestReadResult Read(string document)
    {
        JsonDocument parsed;
        try
        {
            parsed = JsonDocument.Parse(document, new JsonDocumentOptions { MaxDepth = 32 });
        }
        catch (JsonException)
        {
            return Refused(0, [], ["document"], "The plugin manifest is malformed.");
        }
        using (parsed)
        {
            return Read(parsed.RootElement);
        }
    }

    /// <summary>Reads a plugin manifest document.</summary>
    public static PortablePluginManifestReadResult Read(ReadOnlyMemory<byte> utf8Document)
    {
        JsonDocument parsed;
        try
        {
            parsed = JsonDocument.Parse(utf8Document, new JsonDocumentOptions { MaxDepth = 32 });
        }
        catch (JsonException)
        {
            return Refused(0, [], ["document"], "The plugin manifest is malformed.");
        }
        using (parsed)
        {
            return Read(parsed.RootElement);
        }
    }

    private static PortablePluginManifestReadResult Read(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object || HasDuplicateProperties(root))
            return Refused(0, [], ["document"], "The plugin manifest is malformed.");
        if (!root.TryGetProperty("schemaVersion", out var schema) || !schema.TryGetInt32(out var version))
            return Refused(0, [], ["schemaVersion"], "The manifest does not declare a schema version.");
        if (version is not (ManifestSchemaVersion or MultiContributionManifestSchemaVersion or AssetManifestSchemaVersion))
            return Refused(version, [], ["schemaVersion"], $"The manifest schema version {version} is unsupported.");

        var refusedMembers = RefusedMembers(root, version);
        if (refusedMembers.Count > 0)
            return Refused(version, refusedMembers, [],
                $"The manifest declares schema version {version}, which does not accept the members: {string.Join(", ", refusedMembers)}.");

        var refusedDeclarations = new List<string>();
        var manifest = ReadDeclaration(root, version, refusedDeclarations);
        if (manifest is null || refusedDeclarations.Count > 0)
            return Refused(version, [], refusedDeclarations,
                $"The manifest declares schema version {version}, which does not accept: {string.Join(", ", refusedDeclarations)}.");

        return new(manifest, version, [], [], "The manifest was accepted.");
    }

    /// <summary>The document members that the declared schema version refuses.</summary>
    private static List<string> RefusedMembers(JsonElement root, int version)
    {
        var refused = new List<string>();
        if (version == ManifestSchemaVersion)
        {
            if (root.TryGetProperty("installationUnit", out _)) refused.Add("installationUnit");
            if (root.TryGetProperty("definitionFiles", out _)) refused.Add("definitionFiles");
            if (root.TryGetProperty("contributions", out var contributions) && contributions.ValueKind == JsonValueKind.Object)
                foreach (var name in new[] { "clients", "harnessExtensions", "cliHosts", "cliCommands" })
                    if (contributions.TryGetProperty(name, out _)) refused.Add("contributions." + name);
        }
        if (version < AssetManifestSchemaVersion && root.TryGetProperty("assets", out _)) refused.Add("assets");
        return refused;
    }

    private static PortablePluginManifestDeclaration? ReadDeclaration(JsonElement root, int version, List<string> refused)
    {
        var key = ReadText(root, "key");
        var name = ReadText(root, "name");
        var description = ReadText(root, "description");
        if (key is null) refused.Add("key");
        if (name is null) refused.Add("name");
        if (description is null) refused.Add("description");

        var dependencies = ReadDependencies(root, refused);
        var slots = ReadSlots(root, refused);
        var routes = ReadRoutes(root, refused);
        var placements = ReadPlacements(root, refused);
        var contributions = ReadContributions(root, refused);
        var assets = ReadAssets(root, refused);
        var installationUnit = ReadInstallationUnit(root, version, refused);
        var definitionFiles = ReadDefinitionFiles(root, version, refused);
        var target = ReadTarget(root, refused);

        if (refused.Count > 0) return null;

        var declaredContributions = contributions!;
        var declaredAssets = assets!;
        if (version < AssetManifestSchemaVersion
            && (declaredAssets.Stylesheets.Count > 0 || declaredAssets.Scripts.Count > 0))
        {
            refused.Add("assets");
            return null;
        }
        if (version >= MultiContributionManifestSchemaVersion)
        {
            if (installationUnit is null)
            {
                refused.Add("installationUnit");
                return null;
            }
            var requirements = installationUnit.RequiredCapabilities;
            if (requirements.Count == 0) refused.Add("installationUnit.requiredCapabilities");
            else
            {
                if ((declaredContributions.CliHosts.Count > 0 || declaredContributions.CliCommands.Count > 0)
                    && !requirements.Contains("cli", StringComparer.Ordinal))
                    refused.Add("installationUnit.requiredCapabilities[cli]");
                if (declaredContributions.HarnessExtensions.Count > 0
                    && !requirements.Contains("harness", StringComparer.Ordinal))
                    refused.Add("installationUnit.requiredCapabilities[harness]");
                if (target!.Target == PortablePluginTarget.Blazor
                    && !requirements.Contains("blazor", StringComparer.Ordinal))
                    refused.Add("installationUnit.requiredCapabilities[blazor]");
            }
        }
        if (target!.Target != PortablePluginTarget.Blazor
            && (routes!.Count > 0 || slots!.Count > 0 || placements!.Count > 0
                || declaredContributions.Components.Count > 0 || declaredContributions.Pages.Count > 0
                || declaredContributions.Themes.Count > 0))
        {
            refused.Add("target");
            return null;
        }

        return new(
            version,
            key!,
            name!,
            description!,
            target,
            installationUnit,
            dependencies!,
            routes!,
            slots!,
            placements!,
            declaredContributions,
            declaredAssets,
            definitionFiles!);
    }

    private static PortablePluginTargetDeclaration? ReadTarget(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("target", out var element) || element.ValueKind == JsonValueKind.Null)
        {
            refused.Add("target");
            return null;
        }
        if (element.ValueKind == JsonValueKind.String)
        {
            var hostTarget = ParseTarget(element.GetString());
            if (hostTarget is null)
            {
                refused.Add("target");
                return null;
            }
            return new(hostTarget.Value, PortablePluginTargetDeclaration.LegacyVersion, PortablePluginTargetExtension.None);
        }
        if (element.ValueKind != JsonValueKind.Object)
        {
            refused.Add("target");
            return null;
        }
        if (!element.TryGetProperty("hostTarget", out var host) || host.ValueKind != JsonValueKind.String)
        {
            refused.Add("target");
            return null;
        }
        var parsed = ParseTarget(host.GetString());
        if (parsed is null)
        {
            refused.Add("target");
            return null;
        }
        var version = PortablePluginTargetDeclaration.LegacyVersion;
        if (element.TryGetProperty("version", out var declared) && declared.TryGetInt32(out var declaredVersion))
            version = declaredVersion;
        var extension = PortablePluginTargetExtension.None;
        if (element.TryGetProperty("extension", out var declaredExtension) && declaredExtension.ValueKind == JsonValueKind.String)
        {
            if (!Enum.TryParse<PortablePluginTargetExtension>(declaredExtension.GetString(), ignoreCase: true, out extension))
            {
                refused.Add("target");
                return null;
            }
        }
        return new(parsed.Value, version, extension);
    }

    private static PortablePluginTarget? ParseTarget(string? text)
        => text switch
        {
            "backend" => PortablePluginTarget.Backend,
            "blazor" => PortablePluginTarget.Blazor,
            _ => null
        };

    private static PortablePluginInstallationUnit? ReadInstallationUnit(JsonElement root, int version, List<string> refused)
    {
        if (!root.TryGetProperty("installationUnit", out var element)) return null;
        if (element.ValueKind == JsonValueKind.Null)
        {
            refused.Add("installationUnit");
            return null;
        }
        if (element.ValueKind != JsonValueKind.Object)
        {
            refused.Add("installationUnit");
            return null;
        }
        var capabilities = ReadStringList(element, "requiredCapabilities", refused);
        var contracts = new Dictionary<string, string>(StringComparer.Ordinal);
        if (element.TryGetProperty("requiredContracts", out var declaredContracts))
        {
            if (declaredContracts.ValueKind != JsonValueKind.Object)
            {
                refused.Add("installationUnit.requiredContracts");
            }
            else
            {
                foreach (var property in declaredContracts.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.String || !ValidText(property.Value.GetString()))
                    {
                        refused.Add("installationUnit.requiredContracts");
                        continue;
                    }
                    contracts[property.Name] = property.Value.GetString()!;
                }
            }
        }
        return new(capabilities, contracts);
    }

    private static IReadOnlyList<PortablePluginDefinitionFile> ReadDefinitionFiles(JsonElement root, int version, List<string> refused)
    {
        if (!root.TryGetProperty("definitionFiles", out var element)) return [];
        if (element.ValueKind == JsonValueKind.Null)
        {
            refused.Add("definitionFiles");
            return [];
        }
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add("definitionFiles");
            return [];
        }
        var files = new List<PortablePluginDefinitionFile>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object
                || !item.TryGetProperty("path", out var path) || path.ValueKind != JsonValueKind.String
                || !item.TryGetProperty("contentHash", out var hash) || hash.ValueKind != JsonValueKind.Object
                || !hash.TryGetProperty("algorithm", out var algorithm) || algorithm.ValueKind != JsonValueKind.String
                || !hash.TryGetProperty("digest", out var digest) || digest.ValueKind != JsonValueKind.String)
            {
                refused.Add("definitionFiles");
                continue;
            }
            var algorithmText = algorithm.GetString()!;
            var digestText = digest.GetString()!;
            if (!ValidText(path.GetString()) || !PortablePluginContentHash.IsSupported(algorithmText) || !IsDigest(digestText))
            {
                refused.Add("definitionFiles");
                continue;
            }
            files.Add(new(path.GetString()!, new PortablePluginContentHash(algorithmText.ToLowerInvariant(), digestText)));
        }
        return files;
    }

    private static bool IsDigest(string digest)
        => digest.Length is 64 or 128 && digest.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static IReadOnlyList<PortablePluginDependencyDeclaration> ReadDependencies(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("dependencies", out var element)) return [];
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add("dependencies");
            return [];
        }
        var dependencies = new List<PortablePluginDependencyDeclaration>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("key", out var key)
                || key.ValueKind != JsonValueKind.String || !ValidText(key.GetString()))
            {
                refused.Add("dependencies");
                continue;
            }
            dependencies.Add(new(key.GetString()!));
        }
        return dependencies;
    }

    private static IReadOnlyList<PortablePluginSlotDeclaration> ReadSlots(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("slots", out var element)) return [];
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add("slots");
            return [];
        }
        var slots = new List<PortablePluginSlotDeclaration>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("key", out var key)
                || key.ValueKind != JsonValueKind.String || !ValidText(key.GetString())
                || !item.TryGetProperty("componentHost", out var host) || host.ValueKind != JsonValueKind.String
                || !ValidText(host.GetString()))
            {
                refused.Add("slots");
                continue;
            }
            slots.Add(new(key.GetString()!, host.GetString()!));
        }
        return slots;
    }

    private static IReadOnlyList<PortablePluginRouteDeclaration> ReadRoutes(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("routes", out var element)) return [];
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add("routes");
            return [];
        }
        var routes = new List<PortablePluginRouteDeclaration>();
        foreach (var item in element.EnumerateArray())
        {
            var route = ReadRoute(item, refused);
            if (route is null) continue;
            routes.Add(route);
        }
        return routes;
    }

    private static PortablePluginRouteDeclaration? ReadRoute(JsonElement item, List<string> refused)
    {
        if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("path", out var path)
            || path.ValueKind != JsonValueKind.String || !ValidText(path.GetString()))
        {
            refused.Add("routes");
            return null;
        }
        var children = new List<PortablePluginRouteDeclaration>();
        if (item.TryGetProperty("children", out var declaredChildren))
        {
            if (declaredChildren.ValueKind != JsonValueKind.Array)
            {
                refused.Add("routes");
                return null;
            }
            foreach (var child in declaredChildren.EnumerateArray())
            {
                var read = ReadRoute(child, refused);
                if (read is null) continue;
                children.Add(read);
            }
        }
        return new(
            path.GetString()!,
            ReadOptionalText(item, "component"),
            ReadOptionalText(item, "redirectTo"),
            children,
            ReadStringList(item, "policies", refused));
    }

    private static IReadOnlyList<PortablePluginComponentPlacementDeclaration> ReadPlacements(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("placements", out var element)) return [];
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add("placements");
            return [];
        }
        var placements = new List<PortablePluginComponentPlacementDeclaration>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("key", out var key)
                || key.ValueKind != JsonValueKind.String || !ValidText(key.GetString())
                || !item.TryGetProperty("slotKey", out var slotKey) || slotKey.ValueKind != JsonValueKind.String
                || !ValidText(slotKey.GetString())
                || !item.TryGetProperty("component", out var component) || component.ValueKind != JsonValueKind.String
                || !ValidText(component.GetString()))
            {
                refused.Add("placements");
                continue;
            }
            var order = 0;
            if (item.TryGetProperty("order", out var declaredOrder) && declaredOrder.TryGetInt32(out var parsedOrder))
                order = parsedOrder;
            var parameters = new Dictionary<string, PortablePluginParameterValue>(StringComparer.Ordinal);
            if (item.TryGetProperty("parameters", out var declaredParameters))
            {
                if (declaredParameters.ValueKind != JsonValueKind.Object)
                {
                    refused.Add("placements");
                    continue;
                }
                foreach (var property in declaredParameters.EnumerateObject())
                {
                    var value = ReadParameter(property.Value);
                    if (value is null)
                    {
                        refused.Add("placements");
                        continue;
                    }
                    parameters[property.Name] = value;
                }
            }
            placements.Add(new(key.GetString()!, slotKey.GetString()!, component.GetString()!, order, parameters,
                ReadStringList(item, "policies", refused)));
        }
        return placements;
    }

    private static PortablePluginParameterValue? ReadParameter(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => new(PortablePluginParameterKind.Text, element.GetString()),
        JsonValueKind.Number => new(PortablePluginParameterKind.Number, element.GetRawText()),
        JsonValueKind.True or JsonValueKind.False => new(PortablePluginParameterKind.Flag, element.GetRawText()),
        JsonValueKind.Null => new(PortablePluginParameterKind.Null, null),
        _ => null
    };

    private static PortablePluginContributionDeclaration? ReadContributions(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("contributions", out var element))
        {
            refused.Add("contributions");
            return null;
        }
        if (element.ValueKind != JsonValueKind.Object)
        {
            refused.Add("contributions");
            return null;
        }
        return new(
            ReadStringList(element, "clients", refused),
            ReadStringList(element, "harnessExtensions", refused),
            ReadStringList(element, "cliHosts", refused),
            ReadStringList(element, "cliCommands", refused),
            ReadStringList(element, "components", refused),
            ReadStringList(element, "themes", refused),
            ReadStringList(element, "pages", refused),
            ReadStringList(element, "services", refused),
            ReadStringList(element, "capabilities", refused),
            ReadStringList(element, "transitions", refused),
            ReadStringList(element, "nodeTypes", refused),
            ReadStringList(element, "dbContexts", refused),
            ReadStringList(element, "dataServices", refused));
    }

    private static PortablePluginClientAssetDeclaration? ReadAssets(JsonElement root, List<string> refused)
    {
        if (!root.TryGetProperty("assets", out var element))
            return new([], []);
        if (element.ValueKind != JsonValueKind.Object)
        {
            refused.Add("assets");
            return null;
        }
        return new(
            ReadStringList(element, "stylesheets", refused),
            ReadStringList(element, "scripts", refused));
    }

    private static IReadOnlyList<string> ReadStringList(JsonElement parent, string name, List<string> refused)
    {
        if (!parent.TryGetProperty(name, out var element)) return [];
        if (element.ValueKind == JsonValueKind.Null)
        {
            refused.Add(name);
            return [];
        }
        if (element.ValueKind != JsonValueKind.Array)
        {
            refused.Add(name);
            return [];
        }
        var values = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || !ValidText(item.GetString()))
            {
                refused.Add(name);
                continue;
            }
            values.Add(item.GetString()!);
        }
        return values;
    }

    private static string? ReadText(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var element) || element.ValueKind != JsonValueKind.String) return null;
        var text = element.GetString();
        return ValidText(text) ? text : null;
    }

    private static string? ReadOptionalText(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var element) || element.ValueKind == JsonValueKind.Null) return null;
        if (element.ValueKind != JsonValueKind.String) return null;
        var text = element.GetString();
        return ValidText(text) ? text : null;
    }

    private static bool ValidText(string? text) => !string.IsNullOrWhiteSpace(text) && text == text.Trim();

    private static bool HasDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            return element.EnumerateObject().Any(property => !names.Add(property.Name) || HasDuplicateProperties(property.Value));
        }
        return element.ValueKind == JsonValueKind.Array && element.EnumerateArray().Any(HasDuplicateProperties);
    }

    private static PortablePluginManifestReadResult Refused(
        int version, IReadOnlyList<string> members, IReadOnlyList<string> declarations, string message)
        => new(null, version, members, declarations, message);
}

/// <summary>
/// Writes a portable plugin manifest declaration back to a manifest document.
/// The writer uses no reflection based serialization, so a trimmed host can
/// write a manifest without the reflection only JSON stack.
/// </summary>
public static class PortablePluginManifestWriter
{
    /// <summary>Writes the declaration as a manifest document.</summary>
    public static string Write(PortablePluginManifestDeclaration manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", manifest.SchemaVersion);
            writer.WriteString("key", manifest.Key);
            writer.WriteString("name", manifest.Name);
            writer.WriteString("description", manifest.Description);
            writer.WritePropertyName("target");
            WriteTarget(writer, manifest.Target);
            writer.WritePropertyName("dependencies");
            writer.WriteStartArray();
            foreach (var dependency in manifest.Dependencies)
            {
                writer.WriteStartObject();
                writer.WriteString("key", dependency.Key);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("routes");
            WriteRoutes(writer, manifest.Routes);
            writer.WritePropertyName("slots");
            writer.WriteStartArray();
            foreach (var slot in manifest.Slots)
            {
                writer.WriteStartObject();
                writer.WriteString("key", slot.Key);
                writer.WriteString("componentHost", slot.ComponentHost);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("placements");
            writer.WriteStartArray();
            foreach (var placement in manifest.Placements)
            {
                writer.WriteStartObject();
                writer.WriteString("key", placement.Key);
                writer.WriteString("slotKey", placement.SlotKey);
                writer.WriteString("component", placement.Component);
                writer.WriteNumber("order", placement.Order);
                writer.WritePropertyName("parameters");
                writer.WriteStartObject();
                foreach (var parameter in placement.Parameters)
                {
                    writer.WritePropertyName(parameter.Key);
                    WriteParameter(writer, parameter.Value);
                }
                writer.WriteEndObject();
                writer.WritePropertyName("policies");
                writer.WriteStartArray();
                foreach (var policy in placement.Policies) writer.WriteStringValue(policy);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WritePropertyName("contributions");
            WriteContributions(writer, manifest.Contributions, manifest.SchemaVersion);
            if (manifest.SchemaVersion >= PortablePluginManifestValidator.AssetManifestSchemaVersion)
            {
                writer.WritePropertyName("assets");
                writer.WriteStartObject();
                writer.WritePropertyName("stylesheets");
                writer.WriteStartArray();
                foreach (var stylesheet in manifest.Assets.Stylesheets) writer.WriteStringValue(stylesheet);
                writer.WriteEndArray();
                writer.WritePropertyName("scripts");
                writer.WriteStartArray();
                foreach (var script in manifest.Assets.Scripts) writer.WriteStringValue(script);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            if (manifest.InstallationUnit is not null)
            {
                writer.WritePropertyName("installationUnit");
                writer.WriteStartObject();
                writer.WritePropertyName("requiredCapabilities");
                writer.WriteStartArray();
                foreach (var capability in manifest.InstallationUnit.RequiredCapabilities) writer.WriteStringValue(capability);
                writer.WriteEndArray();
                writer.WritePropertyName("requiredContracts");
                writer.WriteStartObject();
                foreach (var contract in manifest.InstallationUnit.RequiredContracts)
                {
                    writer.WriteString(contract.Key, contract.Value);
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            if (manifest.SchemaVersion >= PortablePluginManifestValidator.VersionedManifestSchemaVersion)
            {
                writer.WritePropertyName("definitionFiles");
                writer.WriteStartArray();
                foreach (var file in manifest.DefinitionFiles)
                {
                    writer.WriteStartObject();
                    writer.WriteString("path", file.Path);
                    writer.WritePropertyName("contentHash");
                    writer.WriteStartObject();
                    writer.WriteString("algorithm", file.ContentHash.Algorithm);
                    writer.WriteString("digest", file.ContentHash.Digest);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteTarget(Utf8JsonWriter writer, PortablePluginTargetDeclaration target)
    {
        if (target.Version == PortablePluginTargetDeclaration.LegacyVersion
            && target.Extension == PortablePluginTargetExtension.None)
        {
            writer.WriteStringValue(target.Target == PortablePluginTarget.Blazor ? "blazor" : "backend");
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("hostTarget", target.Target == PortablePluginTarget.Blazor ? "blazor" : "backend");
        writer.WriteNumber("version", target.Version);
        writer.WriteString("extension", target.Extension.ToString());
        writer.WriteEndObject();
    }

    private static void WriteRoutes(Utf8JsonWriter writer, IReadOnlyList<PortablePluginRouteDeclaration> routes)
    {
        writer.WriteStartArray();
        foreach (var route in routes)
        {
            writer.WriteStartObject();
            writer.WriteString("path", route.Path);
            if (route.Component is not null) writer.WriteString("component", route.Component);
            if (route.RedirectTo is not null) writer.WriteString("redirectTo", route.RedirectTo);
            writer.WritePropertyName("children");
            WriteRoutes(writer, route.Children);
            writer.WritePropertyName("policies");
            writer.WriteStartArray();
            foreach (var policy in route.Policies) writer.WriteStringValue(policy);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void WriteContributions(Utf8JsonWriter writer, PortablePluginContributionDeclaration contributions, int version)
    {
        writer.WriteStartObject();
        if (version >= PortablePluginManifestValidator.MultiContributionManifestSchemaVersion)
        {
            WriteStrings(writer, "clients", contributions.Clients);
            WriteStrings(writer, "harnessExtensions", contributions.HarnessExtensions);
            WriteStrings(writer, "cliHosts", contributions.CliHosts);
            WriteStrings(writer, "cliCommands", contributions.CliCommands);
        }
        WriteStrings(writer, "components", contributions.Components);
        WriteStrings(writer, "themes", contributions.Themes);
        WriteStrings(writer, "pages", contributions.Pages);
        WriteStrings(writer, "services", contributions.Services);
        WriteStrings(writer, "capabilities", contributions.Capabilities);
        WriteStrings(writer, "transitions", contributions.Transitions);
        WriteStrings(writer, "nodeTypes", contributions.NodeTypes);
        WriteStrings(writer, "dbContexts", contributions.DbContexts);
        WriteStrings(writer, "dataServices", contributions.DataServices);
        writer.WriteEndObject();
    }

    private static void WriteParameter(Utf8JsonWriter writer, PortablePluginParameterValue value)
    {
        switch (value.Kind)
        {
            case PortablePluginParameterKind.Text:
                writer.WriteStringValue(value.Text);
                break;
            case PortablePluginParameterKind.Number:
                writer.WriteRawValue(value.Text ?? "0", skipInputValidation: true);
                break;
            case PortablePluginParameterKind.Flag:
                writer.WriteRawValue(value.Text ?? "false", skipInputValidation: true);
                break;
            default:
                writer.WriteNullValue();
                break;
        }
    }

    private static void WriteStrings(Utf8JsonWriter writer, string name, IReadOnlyList<string> values)
    {
        writer.WritePropertyName(name);
        writer.WriteStartArray();
        foreach (var value in values) writer.WriteStringValue(value);
        writer.WriteEndArray();
    }
}
