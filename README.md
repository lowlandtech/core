# LowlandTech.Core

Canonical source for the LowlandTech.Core package, extracted from Foundry.
The assembly identity and existing namespaces are retained. The current library
includes graph models/registries, shared application/plugin contracts and its
existing EF Core/crypto dependencies; this extraction does not redesign them.

- src/lib: library source.
- src/test: focused extraction regression tests.
- src/consumer: isolated PackageReference smoke consumer.
- [Extraction provenance and cutover](docs/workitems/done/CORE0001.US01.SC01.verified-package-producer.md).
- [Roadmap](docs/workitems/README.md).

Build with .NET SDK 10.0.300:

~~~powershell
dotnet build Core.slnx -c Release
dotnet test --project src/test/LowlandTech.Core.Tests.csproj -c Release --no-build
pwsh -NoProfile -File eng/build-local.ps1
~~~

Components owns LowlandTech.Components.Core; Core references the exact
2026.10.1-20261024235136415 package from the configured local feed. No sibling
source checkout or duplicate Components.Core producer is part of this solution.
Public NuGet/CI feed provisioning remains separate from local-feed verification.

Earlier public Core commits remain in Git history. Current source was imported
as a reviewed snapshot from private Foundry; private source history is not
reachable from this public branch. Existing package versions stay immutable.

Shared operating guidance lives in the workspace skills; native Core build,
extraction verification and package-consumer tools remain under eng/.
