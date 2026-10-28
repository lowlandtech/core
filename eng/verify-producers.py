"""Prove that Core is the only producer of the Core package identities.

The canonical producer of ``LowlandTech.Core`` is ``src/lib`` and the canonical
producer of ``LowlandTech.Core.Contracts`` is ``src/contracts``. Every other
checkout that can still publish one of those package identities is a competing
producer, and the retirement of each one belongs to the lane that owns that
checkout. This tool measures that state, so the retirement evidence is
reproducible instead of a hand written note.

The tool reads the effective MSBuild properties of a project, because a
checkout can publish a package while its own project file says it cannot: the
``IsPackable`` value of a ``Directory.Build.props`` is overridden by the
project body, and a project can refuse a forced pack with an error target.

Exit codes:
  0  the canonical producer is the only producer
  1  a Core owned violation: the canonical producer is missing, misidentified
     or a second producer exists inside the Core repository
  2  the Core owned invariants hold, but a competing producer outside the Core
     repository can still publish; the evidence names the owner of each one
  3  the tool could not run
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path

#: The package identities Core produces. Components owns ``LowlandTech.Components.Core``.
CORE_PACKAGE_IDS = ("LowlandTech.Core", "LowlandTech.Core.Contracts")

#: Directories that never hold a source checkout of a competing producer.
PRUNED_DIRECTORIES = frozenset({
    ".agent-hub", ".chatbox", ".git", ".skills", ".vs",
    "artifacts", "bin", "node_modules", "obj", "packages", "samples",
})

#: The owner of a competing producer, keyed by the name of its checkout.
REPOSITORY_OWNERS = {
    "backend": "backend",
    "builder": "builder",
    "coder": "coder",
    "components": "components",
    "core": "core",
    "develop": "foundry",
    "harness": "harness",
    "plugins": "plugins",
}

#: The canonical producer of each package identity, relative to the repository root.
CANONICAL_PRODUCERS = {
    "LowlandTech.Core": "src/lib/LowlandTech.Core.csproj",
    "LowlandTech.Core.Contracts": "src/contracts/LowlandTech.Core.Contracts.csproj",
}

_TARGET_PATTERN = re.compile(r"<Target\b[^>]*>.*?</Target>", re.DOTALL)
_BEFORE_TARGETS_PATTERN = re.compile(r'BeforeTargets\s*=\s*"([^"]*)"')
_PACKAGE_ID_PATTERN = re.compile(
    r"<PackageId\b[^>]*>\s*("
    + "|".join(re.escape(identity) for identity in CORE_PACKAGE_IDS)
    + r")\s*</PackageId>"
)

#: The MSBuild files that can hand a package identity down to a project.
_DIRECTORY_BUILD_FILES = ("Directory.Build.props", "Directory.Build.targets")


def _inherited_identities(project: Path) -> set[str]:
    """The Core identities a project inherits from a Directory.Build file."""
    inherited: set[str] = set()
    for directory in [project.parent, *project.parents]:
        for name in _DIRECTORY_BUILD_FILES:
            candidate = directory / name
            if not candidate.is_file():
                continue
            try:
                text = candidate.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            for match in _PACKAGE_ID_PATTERN.finditer(text):
                inherited.add(match.group(1))
    return inherited


def candidates(workspace: Path) -> list[Path]:
    """Every project file that can publish a Core package identity.

    A project qualifies when it declares a Core identity in its own body, when
    it inherits one from a ``Directory.Build`` file, or when it is named after
    an identity and therefore takes it as the package default. A project that
    only *references* a Core package is a consumer, not a producer, so it is
    never a candidate.
    """
    found: list[Path] = []
    for directory, subdirectories, files in os.walk(workspace):
        subdirectories[:] = sorted(
            name for name in subdirectories if name not in PRUNED_DIRECTORIES
        )
        for name in files:
            if not name.endswith(".csproj"):
                continue
            project = Path(directory) / name
            try:
                text = project.read_text(encoding="utf-8", errors="replace")
            except OSError:
                continue
            declares = _PACKAGE_ID_PATTERN.search(text) is not None
            inherits = bool(_inherited_identities(project))
            named_after = project.stem in CORE_PACKAGE_IDS
            if declares or inherits or named_after:
                found.append(project)
    return found


def effective_properties(project: Path) -> dict[str, str]:
    """The evaluated MSBuild properties of a project, or an empty mapping."""
    command = [
        "dotnet", "msbuild", str(project), "-nologo",
        "-getProperty:IsPackable", "-getProperty:PackageId", "-getProperty:Version",
    ]
    try:
        completed = subprocess.run(
            command, capture_output=True, text=True, cwd=str(project.parent), timeout=300
        )
    except (OSError, subprocess.TimeoutExpired):
        return {}
    if completed.returncode != 0:
        return {}
    start = completed.stdout.find("{")
    end = completed.stdout.rfind("}")
    if start < 0 or end < start:
        return {}
    try:
        properties = json.loads(completed.stdout[start:end + 1])
    except json.JSONDecodeError:
        return {}
    return {key: str(value) for key, value in properties.get("Properties", {}).items()}


def refuses_a_forced_pack(project: Path) -> bool:
    """Whether the project rejects a pack that forces ``IsPackable`` to true."""
    try:
        text = project.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return False
    for target in _TARGET_PATTERN.findall(text):
        before = _BEFORE_TARGETS_PATTERN.search(target)
        if before is None:
            continue
        if not any(part.strip() in {"Pack", "GenerateNuspec"}
                   for part in before.group(1).split(";")):
            continue
        if "<Error " in target:
            return True
    return False


def owner_of(project: Path, workspace: Path) -> str:
    """The lane that owns the checkout a project lives in."""
    for parent in [project.parent, *project.parents]:
        if not (parent / ".git").exists():
            continue
        # The checkout may be a worktree inside another checkout, so the owner
        # is the top level workspace directory that contains it.
        for ancestor in [parent, *parent.parents]:
            if ancestor.parent == workspace:
                return REPOSITORY_OWNERS.get(ancestor.name, ancestor.name)
            if ancestor == workspace:
                break
        return REPOSITORY_OWNERS.get(parent.name, parent.name)
    return "unknown"


def describe(project: Path, repository: Path, workspace: Path) -> dict[str, object] | None:
    """The measured state of one project that can publish a Core package identity.

    ``None`` means the project does not publish a Core identity after all: a
    consumer that mentions a Core package, or a project whose declaration is
    overridden elsewhere, publishes something else.
    """
    properties = effective_properties(project)
    if not properties:
        return {
            "project": str(project),
            "evaluated": False,
            "packageId": None,
            "version": None,
            "packable": None,
            "refusesForcedPack": refuses_a_forced_pack(project),
            "classification": "unevaluated",
        }
    package_id = properties.get("PackageId", "")
    if package_id not in CORE_PACKAGE_IDS:
        return None
    packable = properties.get("IsPackable", "").lower() == "true"
    refuses = refuses_a_forced_pack(project)
    canonical = CANONICAL_PRODUCERS.get(package_id)
    relative = project.relative_to(repository) if project.is_relative_to(repository) else project
    if canonical is not None and str(relative).replace("\\", "/") == canonical:
        classification = "canonical" if packable else "canonical-not-packable"
    elif project.is_relative_to(repository):
        classification = "core-owned-competing-producer"
    elif not packable or refuses:
        classification = "retired"
    else:
        classification = "competing-producer"
    return {
        "project": str(project),
        "evaluated": True,
        "packageId": package_id,
        "version": properties.get("Version"),
        "packable": packable,
        "refusesForcedPack": refuses,
        "classification": classification,
        "owner": owner_of(project, workspace),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repository-root", default=str(Path(__file__).resolve().parents[1]))
    parser.add_argument("--workspace-root", default=None)
    parser.add_argument("--json", action="store_true", help="print the evidence as JSON")
    arguments = parser.parse_args()

    repository = Path(arguments.repository_root).resolve()
    workspace = Path(arguments.workspace_root).resolve() if arguments.workspace_root else repository.parent
    if not repository.is_dir():
        print(f"the repository root {repository} does not exist", file=sys.stderr)
        return 3
    if not workspace.is_dir():
        print(f"the workspace root {workspace} does not exist", file=sys.stderr)
        return 3

    measured = [describe(project, repository, workspace) for project in candidates(workspace)]
    measured = [entry for entry in measured if entry is not None]
    measured.sort(key=lambda entry: str(entry["project"]))

    canonical = [entry for entry in measured if str(entry["classification"]).startswith("canonical")]
    core_violations = [entry for entry in measured
                       if entry["classification"] in {"core-owned-competing-producer", "canonical-not-packable"}]
    external_live = [entry for entry in measured if entry["classification"] == "competing-producer"]
    external_retired = [entry for entry in measured if entry["classification"] == "retired"]
    unevaluated = [entry for entry in measured if entry["classification"] == "unevaluated"]

    missing = [identity for identity in CORE_PACKAGE_IDS
               if not any(entry["packageId"] == identity for entry in canonical)]

    evidence = {
        "workspaceRoot": str(workspace),
        "repositoryRoot": str(repository),
        "canonical": canonical,
        "coreViolations": core_violations,
        "competingProducers": external_live,
        "retired": external_retired,
        "unevaluated": unevaluated,
        "missingCanonicalProducers": missing,
    }

    if arguments.json:
        print(json.dumps(evidence, indent=2))
    else:
        print("Core producer uniqueness")
        print(f"  workspace: {workspace}")
        for entry in canonical:
            print(f"  canonical: {entry['packageId']} {entry['version']} "
                  f"<- {Path(str(entry['project'])).relative_to(repository)}")
        for entry in core_violations:
            print(f"  VIOLATION: {entry['packageId']} {entry['version']} "
                  f"<- {entry['project']}")
        for entry in external_live:
            print(f"  competing producer: {entry['packageId']} {entry['version']} "
                  f"<- {entry['project']} (owner: {entry['owner']})")
        for entry in external_retired:
            print(f"  retired: {entry['packageId']} {entry['version']} "
                  f"<- {entry['project']} (owner: {entry['owner']})")
        for entry in unevaluated:
            print(f"  UNEVALUATED: {entry['project']}")
        for identity in missing:
            print(f"  VIOLATION: the canonical producer of {identity} is missing")

    if missing or core_violations:
        return 1
    if unevaluated:
        return 3
    if external_live:
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
