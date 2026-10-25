# Core extraction and producer boundary

Date: 2026-10-25 (Europe/Amsterdam).
Source: private lowlandtech/foundry, c7cb4e12d48173f13dd40bc94c97623372cec4b4,
src/core. Destination: public lowlandtech/core, develop.
Previous public baseline: 30c16f8db97eff24842cdd74818cb3e28f69d4f9.

401 tracked source files were mapped to src/lib. The source checkout and its
unrelated working changes are preserved. source-snapshot.json records per-file
source hashes; the library project is intentionally adapted for standalone
packaging. Runtime C# source remains unchanged.

The public branch preserves the previous public repository ancestry and imports
only the selected current snapshot. Filtered private source history stays in
the local refs/extraction/private-foundry-core ref and must never be pushed.
Historical samples/tests are retained in public Git history; they do not prove
the current graph/plugin library and are replaced in the current solution by
focused current-library tests and a real package-consuming executable.

## Producer and dependency direction

Core is the sole forward package producer for LowlandTech.Core.
Components is the sole producer of LowlandTech.Components.Core.
The extracted Core consumes exactly [2026.10.1-20261024235136415] instead of the
old sibling ProjectReference. No copied Components contract types or weakened
version constraints are introduced.

Foundry/Builder source copies remain compatibility inputs until their owners
cut over; they must not publish competing versions. Consumers must verify new
package pins with their own behavior and closure tests. Components must repack
its remaining batch against the new Core before the shared feature feed is
called repaired. Core-only consumer proof is not that whole-batch proof.

## Verification

The extraction run records actual build, test, package archive and isolated
consumer outcomes here after they execute. No broad Foundry test-suite
conformance, application migration, public package publication or deployment is
claimed. New CI/public feed configuration requires a real source for the exact
Components.Core dependency, not a silently changed version.

Standalone preflight: Core.slnx Release build exits 0 with zero warnings/errors;
four focused regression tests pass with zero skips. eng/build-local.ps1
-SkipPublish exits 0: package archive has the exact Components.Core pin and
no bundled duplicate assembly; a clean external consumer restores into an
empty cache, compiles with zero warnings/errors and runs the graph registry
and ComponentsStartupMap from the actual package. This preflight did not
publish a package; final immutable version/evidence follows after checkpoint.

## Verified local package

Published immutably to the shared local feed:
LowlandTech.Core 2026.10.3-20261025011050827978557 from source 43c4c711af9aa149fb42e85f864cabfa61f878c6.
Package SHA256: c6d0497a4321a7569ff289173496312b8a4484dfd21abdd6269acdf5085da999.
Exact Components.Core dependency: [2026.10.1-20261024235136415].
Final build and isolated consumer build have zero warnings/errors; four tests
pass with zero skips. The consumer restores into an empty cache and runs the
graph registry and ComponentsStartupMap from package references only.
Structured evidence: verification.json. All 400 imported library files match
the recorded source; only project wiring was adapted.

Public GitHub source push is pending explicit private-to-public approval after
automatic review rejected the first attempt. Nothing was pushed to Core.
The filtered private history remains local and excluded from branch ancestry.
