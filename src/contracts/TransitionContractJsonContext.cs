using System.Text.Json.Serialization;
using LowlandTech.Core.Contracts.Plugins;
using LowlandTech.Core.Contracts.Transitions;

namespace LowlandTech.Core.Contracts;

/// <summary>
/// The source generated serializer context of the contract. The contract uses a
/// generated context so that no reflection based serialization is reachable and
/// the assembly stays usable in a trimmed host.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(TransitionDefinitionDocument))]
[JsonSerializable(typeof(TransitionAspectSet))]
[JsonSerializable(typeof(PluginIdentity))]
[JsonSerializable(typeof(PluginRevision))]
[JsonSerializable(typeof(PluginScope))]
[JsonSerializable(typeof(PluginContentHash))]
public sealed partial class TransitionContractJsonContext : JsonSerializerContext;
