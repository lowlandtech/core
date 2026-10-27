| Milestone | Title | Gate |
|---|---|---|
| M1 | Standalone canonical Core producer | Package/Consumer |
| M2 | Producer retirement and source-owned consumer cutover | Producer/Consumer |
| M3 | Shared graph and plugin contracts | Package/Consumer |
| M4 | Legacy Core retirement and EF-free package closure | Package/Consumer |
| M5 | Agentic-gherkin test layout and regression parity | Test/Layout |

| Work | Scope | Title | Priority | Lane | Depends on | Status | Refined |
|---|---|---|---|---|---|---|---|
| M1-001 | CORE0001-US01-SC01 | [Standalone source and verified package producer](done/CORE0001.US01.SC01.verified-package-producer.md) | P0 | core | None | done | true |
| M2-001 | CORE0001-US02-SC01 | [Retire legacy producer and coordinate consumer cutover](doing/CORE0001.US02.SC01.producer-retirement-and-consumer-cutover.md) | P0 | core | M1-001 | doing | true |
| M3-001 | CORE0002-US01-SC01 | [Publish Backend-native graph definition contract](doing/CORE0002.US01.SC01.backend-native-graph-contract.md) | P0 | core | M1-001 | doing | true |
| M3-002 | CORE0002-US02-SC01 | [Centralize portable plugin contribution contract](doing/CORE0002.US02.SC01.portable-plugin-contribution-contract.md) | P0 | core | M3-001 | doing | true |
| M4-001 | CORE0003-US01-SC01 | [Retire unused legacy Core APIs and EF dependency](todo/CORE0003.US01.SC01.legacy-api-and-ef-retirement.md) | P1 | core | M2-001, M3-001, M3-002 | blocked | false |
| M5-001 | CORE0004-US01-SC01 | [Bring Core tests into the agentic-gherkin layout](todo/CORE0004.US01.SC01.agentic-gherkin-test-layout.md) | P1 | core | M1-001 | todo | false |
