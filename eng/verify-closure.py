"""Measure the dependency closure of the canonical Core producer.

``src/lib`` declares an exact dependency on ``LowlandTech.Components.Core``, the
package that the Components lane owns. That exact pin is the generation the
Core package and its isolated consumer are proved against, and it has to stay
unambiguous: a consumer that resolves the dependency has to receive the pinned
generation whichever rule its resolver applies.

NuGet resolves a direct dependency to the lowest applicable version, and a bare
version is a lower bound. A publisher that resolves the highest version of the
feed instead takes a different generation, and that is what the Backend lane
measured in its deployment before its publisher learned to read the exact
version that a producer declares. The versions of this workspace also compare
by their numeric parts, so a generation published with an older timestamp can
sort above a newer one.

This tool measures that closure instead of narrating it. It reports the
version the producer pins, the versions the feed offers, the generation that
a direct reference resolves to, the versions that shadow the pin, and the
reference that the newest published batch of the Components lane declares. A
version that shadows the pin is a finding for the lane that owns the
dependency, because the producer cannot close it alone.

Exit codes:
  0  the pinned generation is unambiguous and no divergence is reported
  1  a Core owned violation: the producer does not declare an exact pin, or the
     pinned generation is not in the feed
  2  the Core owned invariants hold, but another generation of the dependency
     shadows the pin or the newest published batch resolves another one; the
     evidence names the owner of that package
  3  the tool could not run
"""

from __future__ import annotations

import argparse
import json
import re
import sys
import xml.etree.ElementTree as ElementTree
import zipfile
from pathlib import Path

#: The dependency that the canonical producer declares. Components owns it.
DEPENDENCY_ID = "LowlandTech.Components.Core"

#: The lane that owns the dependency package.
DEPENDENCY_OWNER = "components"

#: The batch of the Components lane that publishes the dependency and pins Core.
CONSUMER_PACKAGE_ID = "LowlandTech.Components"

#: The producer that declares the exact dependency.
PRODUCER_PROJECT = Path("src/lib/LowlandTech.Core.csproj")

_PACKAGE_REFERENCE_PATTERN = re.compile(
    r"<PackageReference\b[^>]*\bInclude\s*=\s*\"LowlandTech\.Components\.Core\"[^>]*>",
    re.DOTALL,
)
_VERSION_ATTRIBUTE_PATTERN = re.compile(r"\bVersion\s*=\s*\"([^\"]*)\"")


def _version_key(version: str) -> tuple:
    """A sort key that matches the NuGet ordering of the versions of this feed.

    The versions of this workspace are ``major.minor.patch-<digits>``. NuGet
    compares the numeric parts first, so ``2026.10.2-202610242`` sorts above
    ``2026.10.1-20261027141438913274721`` even though it is the older
    generation of the library. The release label only decides between two
    versions that share the same numeric parts.
    """
    numeric, _, release = version.partition("-")
    parts = []
    for part in numeric.split("."):
        try:
            parts.append(int(part))
        except ValueError:
            parts.append(-1)
    while len(parts) < 4:
        parts.append(0)
    if release.isdigit():
        return (tuple(parts), 1, int(release), "")
    return (tuple(parts), 0, 0, release)


def _publication_key(version: str) -> tuple:
    """A sort key that follows the order in which the versions were published.

    The stamps of this workspace carry the publication date in their first
    eight digits and the time behind it when the stamp is long enough. The
    release train number of the version does not decide that order: the
    packages of the ``2026.10.2`` train were published before the packages of
    the ``2026.10.1`` train, and the NuGet order of the numeric parts puts
    them the other way around.
    """
    _, _, release = version.partition("-")
    date = int(release[:8]) if release.isdigit() and len(release) >= 8 else 0
    time = int(release[8:14]) if release.isdigit() and len(release) >= 14 else 0
    return (date, time, release)


def declared_pin(repository: Path) -> str | None:
    """The exact ``LowlandTech.Components.Core`` pin the producer declares."""
    project = repository / PRODUCER_PROJECT
    try:
        text = project.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    for reference in _PACKAGE_REFERENCE_PATTERN.findall(text):
        match = _VERSION_ATTRIBUTE_PATTERN.search(reference)
        if match is None:
            continue
        version = match.group(1).strip()
        if version.startswith("[") and version.endswith("]"):
            return version[1:-1]
        return version
    return None


def feed_versions(feed: Path) -> list[str]:
    """The versions of the dependency that the local feed offers."""
    if not feed.is_dir():
        return []
    versions = []
    for package in feed.glob(f"{DEPENDENCY_ID}.*.nupkg"):
        version = package.name[len(DEPENDENCY_ID) + 1: -len(".nupkg")]
        if version:
            versions.append(version)
    return sorted(versions, key=_version_key)


def resolve(reference: str, available: list[str]) -> str | None:
    """The version that a reference resolves to.

    An exact reference resolves to itself, and a bare reference is a lower
    bound that NuGet resolves to the lowest applicable version.
    """
    if not reference:
        return None
    if reference.startswith("[") and reference.endswith("]"):
        exact = reference[1:-1]
        return exact if exact in available else None
    candidates = [version for version in available
                   if _version_key(version) >= _version_key(reference)]
    return min(candidates, key=_version_key) if candidates else None


def newest_consumer_batch(feed: Path) -> tuple[str, dict[str, str]] | None:
    """The newest published batch of the Components lane and its dependencies."""
    if not feed.is_dir():
        return None
    candidates = []
    for package in feed.glob(f"{CONSUMER_PACKAGE_ID}.*.nupkg"):
        version = package.name[len(CONSUMER_PACKAGE_ID) + 1: -len(".nupkg")]
        if version and version[0].isdigit():
            dependencies = read_dependencies(package)
            if dependencies:
                candidates.append((version, dependencies))
    if not candidates:
        return None
    version, dependencies = max(candidates, key=lambda entry: _publication_key(entry[0]))
    return version, dependencies


def read_dependencies(package: Path) -> dict[str, str]:
    """The dependencies that a published package declares."""
    try:
        with zipfile.ZipFile(package) as archive:
            nuspec = next(name for name in archive.namelist() if name.endswith(".nuspec"))
            root = ElementTree.fromstring(archive.read(nuspec))
    except (OSError, KeyError, StopIteration, zipfile.BadZipFile, ElementTree.ParseError):
        return {}
    dependencies = {}
    for element in root.iter():
        if element.tag.rsplit("}", 1)[-1] == "dependency":
            identity = element.get("id")
            if identity:
                dependencies[identity] = element.get("version") or ""
    return dependencies


def newest_core_candidate(feed: Path) -> str | None:
    """The newest published candidate of the canonical Core producer."""
    if not feed.is_dir():
        return None
    versions = []
    for package in feed.glob("LowlandTech.Core.*.nupkg"):
        version = package.name[len("LowlandTech.Core."): -len(".nupkg")]
        if version and version[0].isdigit():
            versions.append(version)
    return max(versions, key=_publication_key) if versions else None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repository-root", default=str(Path(__file__).resolve().parents[1]))
    parser.add_argument("--workspace-root", default=None)
    parser.add_argument("--feed-root", default=None)
    parser.add_argument("--json", action="store_true", help="print the evidence as JSON")
    arguments = parser.parse_args()

    repository = Path(arguments.repository_root).resolve()
    workspace = Path(arguments.workspace_root).resolve() if arguments.workspace_root else repository.parent
    feed = Path(arguments.feed_root).resolve() if arguments.feed_root else workspace / "packages"
    if not repository.is_dir():
        print(f"the repository root {repository} does not exist", file=sys.stderr)
        return 3
    if not workspace.is_dir():
        print(f"the workspace root {workspace} does not exist", file=sys.stderr)
        return 3

    pin = declared_pin(repository)
    versions = feed_versions(feed)
    highest = versions[-1] if versions else None

    core_violations = []
    if pin is None:
        core_violations.append({
            "kind": "missing-exact-pin",
            "detail": "The producer does not declare an exact dependency on "
                      f"{DEPENDENCY_ID}.",
            "owner": "core",
        })
    elif pin not in versions:
        core_violations.append({
            "kind": "pinned-generation-absent",
            "detail": f"The pinned generation {pin} is not in the feed {feed}.",
            "owner": "core",
        })

    # A version that sorts above the pin shadows it for a publisher that
    # resolves the highest version of the feed.
    shadowing = [version for version in versions
                 if pin is not None and _version_key(version) > _version_key(pin)]

    hazards = []
    if shadowing:
        hazards.append({
            "kind": "shadowing-versions",
            "pinned": pin,
            "winner": highest,
            "shadowingVersions": shadowing,
            "owner": DEPENDENCY_OWNER,
            "detail": "The versions " + ", ".join(shadowing) + " sort above the pinned "
                      f"generation {pin}, so a publisher that resolves the highest "
                      "version of the feed takes another generation.",
        })

    # The batch of the Components lane is what a consumer of the workspace takes,
    # so its own reference to the dependency decides what that consumer receives.
    consumer = None
    core_candidate = newest_core_candidate(feed)
    batch = newest_consumer_batch(feed)
    if batch is not None:
        batch_version, dependencies = batch
        declared = dependencies.get(DEPENDENCY_ID, "")
        resolved = resolve(declared, versions)
        consumer = {
            "package": CONSUMER_PACKAGE_ID,
            "version": batch_version,
            "declaredCore": dependencies.get("LowlandTech.Core", ""),
            "declaredDependency": declared,
            "resolvedDependency": resolved,
            "takesCoreCandidate": resolve(dependencies.get("LowlandTech.Core", ""),
                                           [core_candidate] if core_candidate else []) == core_candidate,
            "resolvesToPinnedGeneration": resolved is not None and resolved == pin,
        }
        if pin is not None and resolved is not None and resolved != pin:
            hazards.append({
                "kind": "batch-closure-divergence",
                "pinned": pin,
                "winner": resolved,
                "owner": DEPENDENCY_OWNER,
                "detail": f"The published batch {CONSUMER_PACKAGE_ID} {batch_version} declares "
                          f"'{declared}' and therefore resolves {resolved} instead of the "
                          f"pinned generation {pin}.",
            })

    evidence = {
        "workspaceRoot": str(workspace),
        "repositoryRoot": str(repository),
        "feedRoot": str(feed),
        "dependencyId": DEPENDENCY_ID,
        "dependencyOwner": DEPENDENCY_OWNER,
        "producerProject": str(PRODUCER_PROJECT).replace("\\", "/"),
        "pinned": pin,
        "pinnedIsExact": pin is not None,
        "pinnedInFeed": pin is not None and pin in versions,
        "availableVersions": versions,
        "directResolution": resolve(pin, versions) if pin else None,
        "highestAvailable": highest,
        "shadowingVersions": shadowing,
        "pinIsUnambiguous": pin is not None and pin in versions and not shadowing,
        "coreCandidate": core_candidate,
        "consumerBatch": consumer,
        "coreViolations": core_violations,
        "externalHazards": hazards,
    }

    if arguments.json:
        print(json.dumps(evidence, indent=2))
    else:
        print("Core dependency closure")
        print(f"  workspace: {workspace}")
        print(f"  feed: {feed}")
        print(f"  dependency: {DEPENDENCY_ID} (owner: {DEPENDENCY_OWNER})")
        print(f"  pinned: {pin}")
        print(f"  available: {', '.join(versions) if versions else '(none)'}")
        print(f"  direct resolution: {evidence['directResolution']}")
        print(f"  highest available: {highest}")
        print(f"  shadowing versions: {', '.join(shadowing) if shadowing else '(none)'}")
        if consumer is not None:
            print(f"  consumer batch: {consumer['package']} {consumer['version']} "
                  f"declares {consumer['declaredDependency']} and resolves {consumer['resolvedDependency']}")
        for violation in core_violations:
            print(f"  VIOLATION: {violation['detail']}")
        for hazard in hazards:
            print(f"  HAZARD: {hazard['detail']} (owner: {hazard['owner']})")

    if core_violations:
        return 1
    if hazards:
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
