# LowlandTech.Core

This repository is the canonical source and package producer for LowlandTech.Core.
Use the existing develop branch and global Git identity/credentials. Preserve
other owners' files/index; stage only owned paths and commit immediately. Never
force-push or publish a second producer from vendored/source reference copies.

src/lib owns the existing library; src/test owns focused contract regression
tests; src/consumer is a PackageReference-only smoke consumer. Preserve assembly,
namespace and public API identities during extraction. Components.Core is owned
by the Components repository and consumed as an exact published package; never
copy its types here or introduce a sibling ProjectReference.

Build/test/pack with the pinned .NET SDK. eng/build-local.ps1 verifies the library,
tests and isolated package consumer before immutable local-feed publication.
Do not overwrite existing package versions. Shared-feed consumer success is
separate from a source build; record exact pins/checks and remaining cutover work.
Remote package publication and production deployment require separate authority.

The current source snapshot came from private Foundry. This public branch retains
the earlier public Core history plus reviewed current source, not private source
history. Never push refs/extraction/* or use --mirror/--all.
