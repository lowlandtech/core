# Core extraction roadmap

Owner core denotes the source repository, not a new registered agent or automation.
Coordinator: coordinator-1. Consumer lanes retain their implementation ownership.
The existing library is extracted without an API redesign; local package proof
and later consumer migration are separate outcomes.

| Work | Title | Owner | Depends on | Status |
| --- | --- | --- | --- | --- |
| M1-001 | [Standalone source and verified package producer](../../README.md) | core | None | review (public push pending) |
| M2-001 | [Retire legacy producer and coordinate consumer cutover](../extraction.md) | core | M1-001 | blocked |
| M2-X01 | [Components exact batch repack and shared-feed consumer](../../../components/docs/evidence/dependency-closure-repair.md) | components | Verified new Core package | blocked (external) |
| M2-X02 | Builder re-vendor and Coder package adoption | builder/coder | Verified producer/Components closure | blocked (external) |

These source-owned external prerequisites are technical artifacts, not
cross-coordinator acknowledgement gates. No existing lane workitem/claim is
reowned here. Exact versions and checks are recorded in extraction.md.
