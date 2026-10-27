using System.Reflection;
using System.Text.Json.Nodes;
using LowlandTech.Core.Contracts.Transitions;

// The consumer is outside every checkout. It restores the contract package
// from a feed into an empty cache and reaches the contract through the
// package alone, the way an external data-defined plugin reaches it.
var resource = "harness-list-main-intents.json";
var host = Assembly.GetExecutingAssembly();
await using var stream = host.GetManifestResourceStream(resource)
    ?? throw new InvalidOperationException($"The fixture '{resource}' is missing from the consumer.");
using var reader = new StreamReader(stream);
var root = JsonNode.Parse(await reader.ReadToEndAsync())?.AsObject()
    ?? throw new InvalidOperationException("The fixture is empty.");
var document = root["document"]?.AsObject()
    ?? throw new InvalidOperationException("The fixture carries no document.");
var canonical = document.ToJsonString();

var contract = TransitionContractDocument.Read(canonical);
var diagnostics = TransitionContractValidator.Validate(contract);
if (diagnostics.Count > 0)
    throw new InvalidOperationException(string.Join("; ", diagnostics.Select(diagnostic => diagnostic.ToString())));

var roundTripped = TransitionContractDocument.Write(contract);
if (!string.Equals(roundTripped, canonical, StringComparison.Ordinal))
    throw new InvalidOperationException("The packaged contract did not round trip the document.");

var assembly = typeof(TransitionDefinitionDocument).Assembly;
Console.WriteLine($"contractAssembly={assembly.GetName().Name}");
Console.WriteLine($"contractVersion={assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
Console.WriteLine($"schema={contract.Schema}");
Console.WriteLine($"kind={contract.Kind}");
Console.WriteLine($"plugin={contract.Plugin}");
Console.WriteLine($"key={contract.Key}");
Console.WriteLine($"capability={contract.Capability}");
Console.WriteLine($"cases={contract.Aspects.UseCase.Cases.Count}");
Console.WriteLine($"diagnostics={diagnostics.Count}");
Console.WriteLine("roundTrip=identical");
