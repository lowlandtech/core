# LowlandTech.Core.Contracts

The portable, data-only graph and transition contract of the LowlandTech
platform. This package carries documents, identities and refusals; it carries
no host runtime.

## What it carries

- `LowlandTech.Core.Contracts.Transitions` — the versioned `gs.transition/v1`
  transition document, its aspects, the client composition aspects, the
  capability reference of a host and the named refusal diagnostics of an
  inadmissible document.
- `LowlandTech.Core.Contracts.Plugins` — the portable plugin identity: key,
  scope, revision and content hash, the implementation binding, the lifetime
  and the provenance.

## What it deliberately does not carry

No EF Core, no ASP.NET Core, no database and no host runtime dependency. The
assembly declares no package dependency at all. The host keeps its graph
entities, its persistence, its kernel and its authorization.

## Compatibility

This package has its own version line. It does not depend on
`LowlandTech.Core`, so an upgrade of the contract never forces an upgrade of
the compatibility package and the resolved closure of every existing
`LowlandTech.Core` consumer stays unchanged.

## Usage

```csharp
var document = TransitionContractDocument.Read(json);
var diagnostics = TransitionContractValidator.Validate(document);
if (diagnostics.Count > 0)
    throw new InvalidDataException(string.Join("; ", diagnostics.Select(d => d.Message)));

var roundTripped = TransitionContractDocument.Write(document);
```

`TransitionContractDocument.Read` and `TransitionContractDocument.Write` use a
source generated serializer context, so the assembly stays free of
reflection-based serialization and is safe for a trimmed host.
