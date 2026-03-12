# Blueprints Domain — Common Context

## Why Blueprints Exists

Traditional SaaS platforms hard-code data models at build time. Adding a field or entity type requires an engineering release. Blueprints removes this bottleneck by providing a self-service, runtime data-modeling layer.

Blueprints is the schema-definition engine of the Dilcore Platform. Tenants design, version, and manage the shape of their business data through API calls — without code deployments or database migrations. Tenants declare what entities exist, what fields they carry, and how they relate to each other. The platform handles storage, validation, and API exposure automatically.

## Design Principles

- **Tenant-scoped schemas**: Every tenant defines its own isolated set of entity types. A CRM tenant creates `Lead`, `Deal`, `Company` while an HR tenant creates `Employee`, `Department`, `LeaveRequest`. No schema leaks across tenants.
- **Self-service modeling**: Business users or AI agents create and evolve entity definitions through the API. No deployment cycle required.
- **Schema name stability**: Each entity and field receives an immutable `schemaName` (camelCase, auto-generated from display name). Display names can change freely; schema names stay constant so downstream systems never break.
- **Inheritance**: An entity can extend another entity, inheriting its field structure. This enables shared base types (e.g., `BaseContact` extended by `Person` and `Organization`).
- **Tagging and discovery**: Every entity definition carries free-form tags for querying, filtering, and categorizing schemas by domain area.
- **Optimistic concurrency**: All mutations require an `eTag` token to prevent conflicting updates. The system rejects writes when the eTag does not match the current version.

## Multi-Tenancy

All Blueprints operations are tenant-scoped. The tenant is identified by the `x-tenant` HTTP header on every request. Each tenant has full data isolation.

Key rules:
- A schema name must be unique **within a tenant** (not globally)
- Pagination, search, and filtering operate within the tenant boundary
- Cross-entity references (like `extendsEntityId`) must point to an entity within the same tenant

## Domain Parts

Blueprints is organized around **domain parts** — distinct business resource types that the module manages. Each domain part has its own context file with JSON structures, business rules, and validation constraints.

### Registry

| Domain Part | Context File | What It Is | Status |
|---|---|---|---|
| **Entity Definitions** | [entities.md](entities.md) | Schema definitions for business objects — names, fields, nesting, tags, inheritance | Implemented |

When new domain parts are added, they must be registered here with a link to their context file.

## Agent Behavior Rules

1. **Schema integrity**: Never produce output that would break schema name stability. Existing schema names are immutable. New fields get auto-generated schema names from display names.
2. **Validation compliance**: All generated payloads must satisfy the validation rules defined for each domain part. Verify constraints before producing output.
3. **Tenant isolation**: Always assume operations are scoped to a single tenant. Never reference entities across tenant boundaries.
4. **Concurrency safety**: Updates require the current eTag. Acknowledge and handle version conflicts.
5. **Domain accuracy**: Use the correct field types based on semantic meaning. Follow the type selection guidance defined per domain part.
6. **Completeness**: Always generate a description for entities. Always add meaningful tags for categorization.
7. **Constraint awareness**: Know and respect all limits (field counts, nesting depth, name lengths, reserved words). If a user request would violate a constraint, explain the issue and suggest an alternative.
