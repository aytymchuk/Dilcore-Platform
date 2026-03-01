# Blueprints Module

## Business Overview

The **Blueprints** module is the schema-definition engine of the Dilcore Platform. It allows platform tenants to design, version, and manage the *shape* of their business data — without writing code or deploying database migrations. Think of it as a runtime data-modelling layer: users declare what entities exist, what fields they carry, and how they relate to each other, and the platform takes care of storage, validation, and API exposure.

### Why Blueprints Exist

Traditional SaaS platforms hard-code their data models at build time. When a customer needs an extra field or a new entity type the engineering team must ship a release. Blueprints remove that bottleneck:

- **Tenant-scoped schemas** — every tenant defines its own set of entity types. A CRM tenant might create `Lead`, `Deal`, and `Company` entities while an HR tenant creates `Employee`, `Department`, and `LeaveRequest`.
- **Self-service modelling** — business users (or AI agents acting on their behalf) create and evolve entity definitions through the API. No deployment cycle required.
- **Inheritance** — an entity can extend another entity, inheriting its field structure. This enables shared base types (e.g. `BaseContact` extended by `Person` and `Organization`).
- **Tagging and discovery** — every entity definition carries free-form tags so consumers can query, filter, and categorize schemas by domain area (`crm`, `billing`, `v2`, etc.).
- **Schema name stability** — each entity and field receives an immutable `schemaName` (camelCase, auto-generated from the display name) that acts as the storage-level identifier. Display names can change freely; schema names stay constant so downstream systems never break.

### Multi-Tenancy

All Blueprints operations are tenant-scoped. The tenant is identified by the `x-tenant` HTTP header. Each tenant has an isolated MongoDB database (resolved by `TenantDatabasePrefixProvider`), so entity definitions never leak across tenants.

### Architecture at a Glance

```text
HTTP Request
  │
  ▼
Blueprints.WebApi          Minimal API endpoints, validation filters
  │
  ▼
Blueprints.Core            MediatR handlers, AutoMapper, behaviours
  │
  ▼
Blueprints.Actors          Orleans grains — single-writer per entity definition
  │
  ▼
Blueprints.Store           MongoDB persistence (documents, repositories)
  │
  ▼
Blueprints.Domain          Pure domain model, schema name generation, limits
```

| Project | Role |
|---|---|
| `Blueprints.Contracts` | Public DTOs, FluentValidation validators, shared with API clients |
| `Blueprints.Domain` | Domain entities (`EntityDefinition`, `FieldDefinition`), value objects, limits, `SchemaNameGenerator` |
| `Blueprints.Actors.Abstractions` | Orleans grain interfaces and serializable DTOs |
| `Blueprints.Actors` | Grain implementations, field-schema processing, custom grain storage |
| `Blueprints.Core` | MediatR command/query handlers, AutoMapper profiles, cross-cutting behaviours |
| `Blueprints.Store` | MongoDB document model and `IEntityDefinitionRepository` implementation |
| `Blueprints.Infrastructure` | External-service integrations (currently thin) |
| `Blueprints.WebApi` | Endpoint mapping (`/blueprints`), DI wiring, module registration |

---

## Blueprints: Entity Definitions

Entity Definitions are the first (and currently the only) resource type managed by the Blueprints module. An Entity Definition describes the schema for a kind of business object — its name, fields, nesting structure, tags, and inheritance chain.

### Capabilities

| Capability | Description |
|---|---|
| **Create** | Define a new entity with display name, optional description, field tree, tags, and optional parent entity (`extendsEntityId`). |
| **Read (single)** | Retrieve one entity definition by ID. |
| **Read (paged list)** | Paginated listing with full-text search on display name, boolean filter on `isAbstract`, and multi-tag filtering. |
| **Update** | Modify display name, description, abstract flag, fields, and tags. Requires the current `eTag` for optimistic concurrency. Schema names of existing fields are preserved; new fields get generated schema names. Returns added/removed field lists. |
| **Delete** | Permanently remove an entity definition and its grain state. |

### Schema Name Generation

Schema names are the stable, storage-safe identifiers for entities and fields. They are derived from the display name using camelCase conversion:

| Display Name | Generated Schema Name |
|---|---|
| `Customer Order` | `customerOrder` |
| `First Name` | `firstName` |
| `My CRM Entity` | `myCrmEntity` |

**Rules:**
- Non-alphanumeric characters are treated as word separators.
- First word is fully lowercased; subsequent words are PascalCased.
- Schema names are immutable after creation. On update, existing fields keep their schema names; only newly added fields get generated names.
- The following names are **reserved** and cannot be used as field schema names: `id`, `eTag`, `createdAt`, `updatedAt`, `isDeleted`, `tenantId`, `schemaName`, `type`.

### Field Types

| Type | Category | Supports Nested Fields |
|---|---|---|
| `String` | Primitive | No |
| `Number` | Primitive | No |
| `Boolean` | Primitive | No |
| `DateTime` | Primitive | No |
| `File` | Primitive | No |
| `Identifier` | Primitive | No |
| `Object` | Complex | Yes (required, min 1 nested field) |
| `Array` | Complex | Yes (required, min 1 nested field) |

### Validation Rules and Limits

| Constraint | Value |
|---|---|
| Display name length | 2 – 128 characters |
| Display name content | Must contain at least one alphanumeric character |
| Description length | Max 200 characters |
| Top-level fields per entity | Max 100 |
| Nested fields per level | Max 50 |
| Maximum nesting depth | 5 levels |
| Tags per entity | Max 20 |
| Tag length | Max 64 characters |
| Tag format | `^[a-zA-Z0-9_-]+$` |
| Schema name duplicates | Not allowed within the same level |
| Schema name reserved words | Rejected at any depth |
| `Object`/`Array` fields | Must have ≥ 1 nested field |
| Primitive fields | Must not have nested fields |

---

## API Reference

**Base path:** `/blueprints/entity-definitions`
**Authentication:** Required (all endpoints)
**Tenant header:** `x-tenant` (required)

### Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/blueprints/entity-definitions` | List entity definitions (paged) |
| `GET` | `/blueprints/entity-definitions/{id}` | Get entity definition by ID |
| `POST` | `/blueprints/entity-definitions` | Create entity definition |
| `PUT` | `/blueprints/entity-definitions/{id}` | Update entity definition |
| `DELETE` | `/blueprints/entity-definitions/{id}` | Delete entity definition |

### GET — List Entity Definitions

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `skip` | int | 0 | Items to skip |
| `take` | int | 20 | Page size |
| `search` | string | — | Full-text search on display name |
| `isAbstract` | bool | — | Filter by abstract flag |
| `tags` | string | — | Comma-separated tag filter (e.g. `crm,billing`) |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "eTag": 1,
      "schemaName": "customerOrder",
      "displayName": "Customer Order",
      "description": "Represents a customer purchase order",
      "isAbstract": false,
      "extendsEntityId": null,
      "fields": [
        {
          "schemaName": "orderNumber",
          "displayName": "Order Number",
          "type": "String",
          "fields": null
        },
        {
          "schemaName": "totalAmount",
          "displayName": "Total Amount",
          "type": "Number",
          "fields": null
        },
        {
          "schemaName": "lineItems",
          "displayName": "Line Items",
          "type": "Array",
          "fields": [
            {
              "schemaName": "productName",
              "displayName": "Product Name",
              "type": "String",
              "fields": null
            },
            {
              "schemaName": "quantity",
              "displayName": "Quantity",
              "type": "Number",
              "fields": null
            }
          ]
        }
      ],
      "tags": ["orders", "crm"],
      "createdAt": "2026-03-01T12:00:00Z",
      "updatedAt": "2026-03-01T12:00:00Z"
    }
  ],
  "totalCount": 1
}
```

### GET — Get Entity Definition by ID

**Response `200 OK`:** single `EntityDefinitionDto` (same shape as an item in the list above).

**Response `404 Not Found`:**

```json
{
  "type": "https://api.dilcore.com/errors/not-found",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Entity definition '3fa85f64-...' does not exist.",
  "errorCode": "NOT_FOUND"
}
```

### POST — Create Entity Definition

**Request body:**

```json
{
  "displayName": "Customer Order",
  "description": "Represents a customer purchase order",
  "isAbstract": false,
  "extendsEntityId": null,
  "schemaName": null,
  "fields": [
    {
      "displayName": "Order Number",
      "type": "String"
    },
    {
      "displayName": "Total Amount",
      "type": "Number"
    },
    {
      "displayName": "Line Items",
      "type": "Array",
      "fields": [
        {
          "displayName": "Product Name",
          "type": "String"
        },
        {
          "displayName": "Quantity",
          "type": "Number"
        }
      ]
    }
  ],
  "tags": ["orders", "crm"]
}
```

| Property | Type | Required | Notes |
|---|---|---|---|
| `displayName` | string | Yes | 2–128 chars, must contain at least one alphanumeric character |
| `description` | string | No | Max 200 chars |
| `isAbstract` | bool | No | Defaults to `false` |
| `extendsEntityId` | GUID | No | ID of the parent entity definition (must exist) |
| `schemaName` | string | No | Custom schema name override; auto-generated from `displayName` when omitted |
| `fields` | array | No | Field definitions (see field structure below); defaults to `[]` |
| `tags` | array | No | String tags; defaults to `[]` |

**Response `201 Created`:** `EntityDefinitionDto` with `Location` header pointing to the new resource.

**Response `409 Conflict`:** when `schemaName` already exists in the tenant.

### PUT — Update Entity Definition

**Request body:**

```json
{
  "eTag": 1,
  "displayName": "Customer Order v2",
  "description": "Updated description",
  "isAbstract": false,
  "fields": [
    {
      "schemaName": "orderNumber",
      "displayName": "Order Number",
      "type": "String"
    },
    {
      "displayName": "Priority",
      "type": "String"
    }
  ],
  "tags": ["orders", "crm", "v2"]
}
```

| Property | Type | Required | Notes |
|---|---|---|---|
| `eTag` | long | Yes | Optimistic concurrency token from the last read |
| `displayName` | string | No | If provided, 2–128 chars |
| `description` | string | No | Max 200 chars; pass `null` to clear |
| `isAbstract` | bool | No | |
| `fields` | array | No | Full replacement of the field tree. Existing fields (matched by `schemaName`) preserve their schema names. New fields get auto-generated schema names. |
| `tags` | array | No | Full replacement |

**Response `200 OK`:** `EntityDefinitionDto`.

**Response `409 Conflict`** (ETag mismatch):

```json
{
  "type": "https://api.dilcore.com/errors/etag-mismatch",
  "title": "Conflict",
  "status": 409,
  "detail": "ETag mismatch for entity definition '...'. Expected 2, got 1.",
  "errorCode": "ETAG_MISMATCH"
}
```

### DELETE — Delete Entity Definition

**Response `200 OK`:** empty body.

**Response `404 Not Found`:** standard ProblemDetails.

### Field Definition Structure (Input / Output)

```json
{
  "schemaName": "firstName",
  "displayName": "First Name",
  "type": "String",
  "fields": null
}
```

| Property | Type | Required on Create | Required on Update | Notes |
|---|---|---|---|---|
| `displayName` | string | Yes | Yes | 2–128 chars |
| `type` | string | Yes | Yes | One of: `String`, `Number`, `Boolean`, `DateTime`, `Object`, `Array`, `File`, `Identifier` |
| `schemaName` | string | No | No (but recommended for existing fields) | Auto-generated from `displayName` if omitted. On update, pass the existing `schemaName` to match and preserve it. |
| `fields` | array | Only for `Object`/`Array` | Only for `Object`/`Array` | Nested field definitions (recursive, same structure). Must have ≥ 1 item for complex types; must be `null`/absent for primitives. |

### Validation Error Response

```json
{
  "type": "https://api.dilcore.com/errors/data-validation-failed",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errorCode": "DATA_VALIDATION_FAILED",
  "errors": {
    "displayName": ["DisplayName is required."],
    "fields[0].type": ["Field type 'Invalid' is not valid. Allowed: String, Number, Boolean, DateTime, Object, Array, File, Identifier."]
  }
}
```

---

## AI Agent Instructions

The sections below provide context and constraints for AI agents operating over the Blueprints Entity Definitions API. Each agent has a distinct responsibility and should follow the guidelines for its role.

---

### Agent 1 — UI/UX Front-End Mockup Agent

**Role:** Design visual mockups and interaction flows for a front-end application that consumes the Entity Definitions API.

#### Context You Must Know

1. **Entity Definition is a schema builder.** Users are creating *types*, not *records*. The UI should feel like a form builder or a database table designer — not like a data-entry form.
2. **Field tree is recursive.** `Object` and `Array` fields contain nested fields up to 5 levels deep. The UI must handle nesting visually (tree view, indented list, or collapsible sections).
3. **Schema names are auto-generated and stable.** Users should never be asked to type schema names. Show them as read-only chips or secondary labels next to the display name.
4. **Tags are free-form labels.** Present them as a tag input (chips with autocomplete from existing tags in the tenant).
5. **Optimistic concurrency via `eTag`.** The front-end must read the current `eTag` before updating, and handle `409 Conflict` gracefully (e.g. "This definition was modified by another user. Reload and try again.").
6. **Pagination and search.** The list endpoint supports `skip`/`take`, `search` on display name, `isAbstract` toggle, and comma-separated `tags` filter. Design a list view with search bar, tag filter pills, and an "Abstract only" toggle.

#### Pages and Flows to Design

| Page | Key Elements |
|---|---|
| **Entity Definition List** | Paginated table/card list, search bar, tag filter, "Abstract only" toggle, create button. Each row shows: display name, schema name (secondary), field count, tags, created/updated dates, actions (edit, delete). |
| **Create Entity Definition** | Form with: display name input, description textarea, "Is Abstract" checkbox, parent entity selector (dropdown filtered to existing entities, optional), tag input, and a **Field Builder** section. Submit calls `POST`. |
| **Edit Entity Definition** | Same form pre-filled from `GET /{id}`. Must display and send `eTag`. Handle `409` with a conflict resolution prompt. Submit calls `PUT`. |
| **Field Builder (nested component)** | A tree/list UI allowing the user to add, remove, and reorder fields. Each field row has: display name input, type dropdown, and (for Object/Array) a nested field builder. Show schema name as a read-only badge. Enforce max depth (5) and max count per level (50) in the UI. |
| **Delete Confirmation** | Modal dialog confirming deletion with the entity name. |

#### Visual Guidelines

- Use a clean, minimal design. Entity definitions are technical objects — prioritize clarity over decoration.
- The field builder is the most complex component. Consider a drag-and-drop tree or an indented list with "add field" buttons at each level.
- Show field type icons (e.g. `Abc` for String, `#` for Number, `{ }` for Object, `[ ]` for Array).
- Surface validation errors inline next to the offending input, using the `errors` map from the API response (keys are dot-paths like `fields[0].displayName`).

---

### Agent 2 — Entity Structure Generation Agent

**Role:** Generate entity definition payloads by interpreting natural-language user requests and producing valid `POST /blueprints/entity-definitions` JSON bodies.

#### API Capabilities You Must Use

- **Endpoint:** `POST /blueprints/entity-definitions`
- **Authentication:** Bearer token (provided externally) + `x-tenant` header.
- **Content-Type:** `application/json`

#### JSON Template

Always generate output conforming to this structure:

```json
{
  "displayName": "<required — 2-128 chars, must contain at least one alphanumeric character>",
  "description": "<optional — max 200 chars>",
  "isAbstract": false,
  "extendsEntityId": "<optional GUID — only if the user explicitly references a parent entity>",
  "fields": [
    {
      "displayName": "<required — 2-128 chars>",
      "type": "<required — one of: String, Number, Boolean, DateTime, Object, Array, File, Identifier>",
      "fields": "<only for Object/Array — recursive array of field definitions; null or omit for primitives>"
    }
  ],
  "tags": ["<optional — alphanumeric, hyphens, underscores; max 64 chars each; max 20 tags>"]
}
```

#### Rules You Must Follow

1. **Never invent schema names.** Omit `schemaName` from the payload entirely — the API auto-generates them from display names.
2. **Field type selection:**
   - `String` — names, labels, descriptions, emails, URLs, addresses, statuses, and any free text.
   - `Number` — quantities, amounts, prices, percentages, scores, and numeric IDs.
   - `Boolean` — yes/no flags, toggles, and binary states.
   - `DateTime` — dates, timestamps, deadlines, and schedules.
   - `File` — attachments, images, documents, and uploads.
   - `Identifier` — references to other entities or external system IDs.
   - `Object` — groups related sub-fields together (e.g. "address with street, city, zip"). Must contain at least one nested field.
   - `Array` — lists of structured items (e.g. "line items" or "phone numbers with label and number"). Must contain at least one nested field.
3. **Nesting limits:** maximum 5 levels deep. Maximum 50 fields per nested level. Maximum 100 top-level fields.
4. **Tags:** if the user mentions a domain area (e.g. "CRM", "HR", "inventory"), add it as a lowercase tag. Always add tags that help categorize the entity.
5. **`isAbstract`:** set to `true` only if the user explicitly says this is a base/abstract entity not meant to hold data directly.
6. **`extendsEntityId`:** include only when the user explicitly names a parent entity *and* you can resolve its ID. If unsure, omit it and note the ambiguity.
7. **`description`:** always generate a concise one-sentence description summarizing the entity's purpose (max 200 chars).
8. **Validation awareness:** before emitting the JSON, mentally verify that all constraints in the validation rules table above are satisfied. If the user's request would violate a constraint, explain the issue and suggest an alternative.

#### Example: User Request and Expected Output

**User:** "Create an entity for tracking customer orders. Each order has a number, date, total amount, status, and a list of line items. Each line item has a product name, quantity, and unit price."

**Generated payload:**

```json
{
  "displayName": "Customer Order",
  "description": "Tracks customer purchase orders with line items.",
  "isAbstract": false,
  "fields": [
    { "displayName": "Order Number", "type": "String" },
    { "displayName": "Order Date", "type": "DateTime" },
    { "displayName": "Total Amount", "type": "Number" },
    { "displayName": "Status", "type": "String" },
    {
      "displayName": "Line Items",
      "type": "Array",
      "fields": [
        { "displayName": "Product Name", "type": "String" },
        { "displayName": "Quantity", "type": "Number" },
        { "displayName": "Unit Price", "type": "Number" }
      ]
    }
  ],
  "tags": ["orders", "crm"]
}
```

#### Handling Ambiguity

When the user's request is vague:
- Ask clarifying questions rather than guessing complex structures.
- For simple cases, make reasonable assumptions and state them explicitly (e.g. "I assumed Status is a String field; let me know if you'd prefer a predefined set of values").
- If the user names a field but doesn't specify the type, infer it from the field name using the type selection rules above.

---

## Getting Started

### Register the Module

```csharp
builder.AddBlueprintsModule();
```

### Map Endpoints

```csharp
app.MapBlueprintsEndpoints();
```

### Configuration

The store requires a MongoDB connection string:

```json
{
  "Blueprints": {
    "MongoDb": {
      "ConnectionString": "mongodb://localhost:27017"
    }
  }
}
```

### Run Tests

```bash
dotnet test tests/Blueprints/
```
