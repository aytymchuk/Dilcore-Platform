---
name: blueprint-context-updater
description: Analyzes the src/Blueprints codebase and updates or creates Blueprint agent context files. Use when new domain parts are added to Blueprints (entities, projections, workflows, views, forms, API integrations, etc.), when structures change, or when the user asks to update Blueprint context files.
---

# Blueprint Context Updater

Analyzes `src/Blueprints` and keeps the Blueprint agent context files in `.cursor/skills/blueprint-agent/` accurate and up-to-date.

## Context Files Layout

```
.cursor/skills/blueprint-agent/
├── SKILL.md              # Skill entry point (never modify directly)
├── common.md             # Shared context — business purpose, design principles, domain parts registry
└── {domain-part}.md      # One file per domain part (entities.md, projections.md, etc.)
```

Each domain part implemented in `src/Blueprints` gets its own `{domain-part}.md` file. The `common.md` file maintains a registry of all domain parts.

## Domain Part Categories

The Blueprints module may contain domain parts from any of these categories. Scan for all of them:

| Category | What It Covers | Example File |
|---|---|---|
| **Entities** | Schema definitions for business objects — fields, types, nesting, inheritance | `entities.md` |
| **Projections** | Read-optimized views derived from entity data — field selection, computed fields | `projections.md` |
| **Workflows** | State machines and process definitions — states, transitions, triggers, actions | `workflows.md` |
| **Views** | Saved query/filter configurations — columns, sorting, grouping, filters | `views.md` |
| **Forms** | UI form layout definitions — sections, field placement, conditional visibility | `forms.md` |
| **API Integrations** | External system connection definitions — endpoints, mappings, authentication | `api-integrations.md` |
| **Relationships** | Links between entity definitions — cardinality, referential rules | `relationships.md` |
| **Constraints** | Cross-field and cross-entity validation rules — uniqueness, required-if, ranges | `constraints.md` |

This list is not exhaustive. Any new domain part found in the codebase that doesn't fit these categories should still get its own file with a descriptive name.

## When to Run

- A new domain part is added to `src/Blueprints`
- An existing domain part's structures change (new properties, new validation rules, new endpoints, new field types)
- New cross-cutting behaviors are added to the pipeline
- The user requests a context refresh or mentions Blueprint documentation is stale

## Workflow

```
Task Progress:
- [ ] Step 1: Scan the Blueprints module for all domain parts
- [ ] Step 2: Read existing context files and build a diff
- [ ] Step 3: Update changed files / create new files
- [ ] Step 4: Update the domain parts registry in common.md
- [ ] Step 5: Verify completeness and consistency
```

### Step 1: Scan the Blueprints Module

Explore `src/Blueprints` to identify every domain part. A domain part is recognized by the presence of **two or more** of these signals:

| Signal | Where to Look |
|---|---|
| WebApi endpoints | `Blueprints.WebApi/` — endpoint mapping methods, route groups |
| Contracts/DTOs | `Blueprints.Contracts/` — subdirectories per domain part |
| MediatR handlers | `Blueprints.Core/Features/` — subdirectories per domain part |
| Domain entities | `Blueprints.Domain/Entities/` — domain model classes |
| Domain limits | `Blueprints.Domain/*Limits.cs` — constraint constants |
| Grain interfaces | `Blueprints.Actors.Abstractions/` — grain interface per domain part |
| Store documents | `Blueprints.Store/Entities/` — MongoDB document models |
| Validators | `Blueprints.Contracts/**/` — FluentValidation validator classes |

For each domain part found, collect:
1. All DTO properties (request and response) — these become the JSON structures
2. All validation rules from validators and limits constants
3. All API endpoints (method, path, query parameters)
4. All behaviors in the MediatR pipeline (uniqueness checks, referential validation, etc.)
5. All error codes and response formats
6. Business semantics — what does this domain part represent and why does it exist

### Step 2: Compare Against Existing Context Files

Read all `*.md` files in `.cursor/skills/blueprint-agent/` and determine:

1. **Documented domain parts**: Parse the "Domain Parts" table in `common.md` to get the current registry
2. **Missing domain parts**: Any domain part found in Step 1 that has no corresponding file
3. **Changed domain parts**: Any domain part where the implementation differs from what is documented — look for added/removed properties, changed validation rules, new/removed endpoints, new behaviors
4. **Stale domain parts**: Any file that references a domain part no longer present in the codebase

### Step 3: Update or Create Context Files

**For existing domain parts that changed** — update the corresponding `{domain-part}.md` file:
- Add/remove properties in JSON structures and property reference tables
- Update validation rules and limits tables
- Add/remove endpoint documentation
- Update field type or enum tables if applicable
- Update behaviors section if pipeline logic changed

**For new domain parts** — create a new file using the template below:
- File name: `{domain-part-name}.md` (lowercase, hyphens)
- Place in `.cursor/skills/blueprint-agent/`

**For stale domain parts** — remove the file and its entry from `common.md`

### Step 4: Update common.md

Update the "Domain Parts" table in `common.md` to reflect the current state:
- Add rows for newly created domain part files
- Remove rows for deleted domain parts
- Update status or description if a domain part's scope changed

### Step 5: Verify

- Every domain part in `src/Blueprints` has a corresponding `.md` file
- All JSON structures match the actual DTOs (no code, only JSON the API expects)
- All validation rules match the actual limits and validators
- All endpoints are documented with correct methods, paths, and parameters
- No implementation-specific details (no language-specific types, no internal class names)
- Consistent terminology across all files
- The domain parts registry in `common.md` matches the actual set of files

## Template for New Domain Part File

When creating a file for a new domain part, follow this structure. Omit sections that don't apply (e.g., a domain part without list pagination skips the query parameters section).

```markdown
# Blueprints Domain — {Domain Part Name}

{One-paragraph description of what this domain part represents, its purpose in the Blueprints module, and how it relates to other domain parts.}

## Base Path

`/blueprints/{resource-path}`

All endpoints require authentication and the `x-tenant` header.

## Operations

| Operation | Method | Path | Description |
|---|---|---|---|
| ... | ... | ... | ... |

---

## {Resource} Structure

### Full {Resource} (Response Format)

{JSON example of the complete response object with representative sample data}

### Property Reference

| Property | Type | Description |
|---|---|---|
| ... | ... | ... |

---

## Validation Rules and Limits

### {Category}-Level Constraints

| Constraint | Value | Why |
|---|---|---|
| ... | ... | ... |

---

## {Operation Name}

### Request Body

{JSON example}

### Request Properties

| Property | Type | Required | Description |
|---|---|---|---|
| ... | ... | ... | ... |

### Responses

- **{status}**: {description}

### Business Rules

{What the system enforces before or after this operation — validation, uniqueness, referential integrity}

---

(Repeat per operation)

---

## Error Formats

### {Error Type} ({status code})

{JSON example}

{Explanation of when this error occurs}
```

## Content Rules

1. **Business logic only**: Every sentence must answer a WHY or WHAT question. No architecture layers, no framework names, no implementation patterns, no internal class names. Describe the domain, not the code.
2. **JSON with semantic context**: Present domain structures as JSON the API expects. Accompany each JSON block with an explanation of what it represents and why it exists — not just its shape, but its business meaning.
3. **No code**: No language-specific types, no internal class names. Use JSON types (string, number, boolean, array, GUID, datetime).
4. **LLM-optimized**: Use tables for structured data, JSON blocks for API contracts, clear headings for navigation. Headings should be scannable for quick lookup.
5. **Comprehensive**: Include all validation rules with their business rationale, all field/value types, all error formats, all edge cases.
6. **Stable references**: Use consistent terminology throughout. Pick one term per concept and use it everywhere.
7. **Cross-references**: When a domain part references another (e.g., a form references entity fields), note the dependency and link to the other domain part file.
