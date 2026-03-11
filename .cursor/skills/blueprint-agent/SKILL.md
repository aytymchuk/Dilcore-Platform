---
name: blueprint-agent
description: Provides domain context for the Blueprints module of the Dilcore Platform. Use when working with Blueprints code, schema modeling, entity definitions, projections, workflows, views, forms, API integrations, or any task involving the src/Blueprints directory.
---

# Blueprint Agent Context

This skill provides business domain knowledge for the Blueprints module — the schema-definition engine of the Dilcore Platform. Context is split across multiple files, one per domain concern. Files describe **what** the system does and **why**, with JSON structures and semantic explanations. No implementation details.

## How to Use

1. **Always read [common.md](common.md) first** — it contains the business purpose, design principles, multi-tenancy rules, the domain parts registry, and agent behavior rules.
2. **Then read the domain part file(s) relevant to your task** — each domain part has its own file (e.g., `entities.md`). The full list is maintained in the "Registry" table inside `common.md`.
3. If the domain part you need is not listed, flag it — the context may need updating via the `blueprint-context-updater` skill.

## File Conventions

| File | Purpose |
|---|---|
| `common.md` | Shared context — business purpose, design principles, tenancy rules, domain parts registry, agent behavior rules |
| `{domain-part}.md` | One file per domain part — JSON structures with semantic explanations, business rules, validation constraints, API contracts |

## Key Principles

1. All content answers **why** and **what** — not how the code is structured internally
2. Schema names are immutable storage identifiers — never break existing schema name contracts
3. All operations are tenant-scoped — never allow cross-tenant data leakage
4. Optimistic concurrency via eTag — always respect version conflicts
5. When a task spans multiple domain parts, read all relevant files before proceeding
