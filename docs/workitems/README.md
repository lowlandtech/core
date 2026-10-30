| Milestone | Title | Gate |
|---|---|---|
| M1 | Standalone canonical Core producer | Package/Consumer |
| M2 | Producer retirement and source-owned consumer cutover | Producer/Consumer |
| M3 | Shared graph and plugin contracts | Package/Consumer |
| M4 | Legacy Core retirement and EF-free package closure | Package/Consumer |
| M5 | Agentic-gherkin test layout and regression parity | Test/Layout |
| M6 | Graph git-flow contract for client surfaces | Contract/Consumer |

| Work | Scope | Title | Priority | Lane | Depends on | Status | Refined | Issue |
|---|---|---|---|---|---|---|---|---|
| M1-001 | CORE0001-US01-SC01 | [Standalone source and verified package producer](done/CORE0001.US01.SC01.verified-package-producer.md) | P0 | core | None | done | true | [#4](https://github.com/lowlandtech/core/issues/4) |
| M2-001 | CORE0001-US02-SC01 | [Retire legacy producer and coordinate consumer cutover](doing/CORE0001.US02.SC01.producer-retirement-and-consumer-cutover.md) | P0 | core | M1-001 | doing | true | [#5](https://github.com/lowlandtech/core/issues/5) |
| M3-001 | CORE0002-US01-SC01 | [Publish Backend-native graph definition contract](done/CORE0002.US01.SC01.backend-native-graph-contract.md) | P0 | core | M1-001 | done | true | [#6](https://github.com/lowlandtech/core/issues/6) |
| M3-002 | CORE0002-US02-SC01 | [Centralize portable plugin contribution contract](done/CORE0002.US02.SC01.portable-plugin-contribution-contract.md) | P0 | core | M3-001 | done | true | [#7](https://github.com/lowlandtech/core/issues/7) |
| M3-003 | CORE0006-US01-SC01 | [Own the transition projection and checkpoint seam decision](todo/CORE0006.US01.SC01.transition-projection-and-checkpoint-seam.md) | P1 | core | M3-001 | todo | true | [#11](https://github.com/lowlandtech/core/issues/11) |
| M4-001 | CORE0003-US01-SC01 | [Retire unused legacy Core APIs and EF dependency](todo/CORE0003.US01.SC01.legacy-api-and-ef-retirement.md) | P1 | core | M2-001, M3-001, M3-002 | blocked | false | [#8](https://github.com/lowlandtech/core/issues/8) |
| M5-001 | CORE0004-US01-SC01 | [Bring Core tests into the agentic-gherkin layout](done/CORE0004.US01.SC01.agentic-gherkin-test-layout.md) | P1 | core | M1-001 | done | true | [#9](https://github.com/lowlandtech/core/issues/9) |
| M6-001 | CORE0005-US01-SC01 | [Publish merge-request and feature-proposal states for clients](done/CORE0005.US01.SC01.git-flow-client-contract.md) | P0 | core | M1-001 | done | true | [#10](https://github.com/lowlandtech/core/issues/10) |
