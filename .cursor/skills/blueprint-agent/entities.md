# Blueprints Domain — Entity Definitions

Entity Definitions are the foundational resource of the Blueprints module. They answer the question: **"What kinds of business objects exist in this tenant?"**

An Entity Definition declares the schema for a type of business object — its name, data fields, nesting structure, classification tags, and inheritance chain. When a tenant creates an Entity Definition called "Customer Order" with fields like "Order Number" and "Line Items," they are telling the platform what shape a Customer Order takes. The platform then knows how to store, validate, and expose instances of that entity.

## Base Path

`/blueprints/entity-definitions`

All endpoints require authentication and the `x-tenant` header.

## Operations

| Operation | Method | Path | Description |
|---|---|---|---|
| List | `GET` | `/blueprints/entity-definitions` | Paginated listing with search and filters |
| Get by ID | `GET` | `/blueprints/entity-definitions/{id}` | Retrieve single entity definition |
| Create | `POST` | `/blueprints/entity-definitions` | Define a new entity type |
| Update | `PUT` | `/blueprints/entity-definitions/{id}` | Modify an existing entity type (requires eTag) |
| Delete | `DELETE` | `/blueprints/entity-definitions/{id}` | Permanently remove an entity type |

---

## Entity Definition Structure

An Entity Definition is the complete description of a business object type. It carries both the human-readable identity (display name, description, tags) and the machine-readable contract (schema name, field tree).

### Full Entity Definition (Response)

```json
{
  "id": "b7e3f1a2-4c8d-4f2a-9b1e-6d5a3c8f2e1b",
  "eTag": 1,
  "schemaName": "customerOrder",
  "displayName": "Customer Order",
  "description": "Represents a customer purchase order with line items and totals",
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
```

### Property Reference

| Property | Type | Description |
|---|---|---|
| `id` | GUID | Unique identifier assigned at creation. Used to reference this entity definition in API calls and cross-entity relationships (like `extendsEntityId`). |
| `eTag` | long | Version counter for optimistic concurrency. Every mutation increments this value. Clients must send the current eTag on updates to prevent conflicting overwrites. |
| `schemaName` | string | Immutable, camelCase storage identifier. Auto-generated from `displayName` at creation. Once set, it never changes — even if the display name is updated. Downstream systems depend on schema name stability. |
| `displayName` | string | Human-readable name shown in UIs and used to generate the `schemaName`. Can be freely renamed without breaking integrations. 2–128 characters. |
| `description` | string or null | Explains what this entity type represents in the tenant's business domain. Helps users and AI agents understand the purpose of the entity. Max 200 characters. |
| `isAbstract` | boolean | When `true`, this entity serves as a base type meant to be extended, not used directly. Abstract entities define shared field structures (e.g., `BaseContact` with common fields like name and email). |
| `extendsEntityId` | GUID or null | Points to a parent entity definition within the same tenant. The child entity inherits the parent's field structure. Enables type hierarchies like `Person` and `Organization` both extending `BaseContact`. |
| `fields` | array | The ordered list of field definitions that describe the data shape of this entity. Fields form a recursive tree — complex types contain nested fields. See [Field Definition Structure](#field-definition-structure). |
| `tags` | array of strings | Free-form labels for categorizing and filtering entity definitions. Used to organize schemas by business domain (e.g., `crm`, `billing`, `hr`). Useful for discovery when a tenant has many entity types. |
| `createdAt` | datetime | UTC timestamp of when this entity definition was first created. |
| `updatedAt` | datetime | UTC timestamp of the most recent modification. |

---

## Field Definition Structure

Fields define what data an entity carries. They form a recursive tree: primitive types (`String`, `Number`, etc.) are leaf nodes, while complex types (`Object`, `Array`) branch into nested fields.

This recursion is what allows rich, structured business data — an "Order" entity can have an "Address" object field containing "Street," "City," and "Zip" sub-fields, or a "Line Items" array field where each item has "Product Name" and "Quantity."

### Field (Within an Entity Definition)

```json
{
  "schemaName": "shippingAddress",
  "displayName": "Shipping Address",
  "type": "Object",
  "fields": [
    {
      "schemaName": "street",
      "displayName": "Street",
      "type": "String",
      "fields": null
    },
    {
      "schemaName": "city",
      "displayName": "City",
      "type": "String",
      "fields": null
    }
  ]
}
```

| Property | Type | Required on Create | Required on Update | Description |
|---|---|---|---|---|
| `schemaName` | string | No | No (recommended for existing fields) | Immutable storage identifier for this field, auto-generated from `displayName` if omitted. On updates, include the existing `schemaName` to preserve field identity — omitting it causes a new schema name to be generated, effectively creating a new field. |
| `displayName` | string | Yes | Yes | Human-readable name for the field. Used to generate `schemaName` when it is not provided. 2–128 characters, must contain at least one alphanumeric character. |
| `type` | string | Yes | Yes | Determines what kind of data this field holds. See [Field Types](#field-types). |
| `fields` | array or null | Only for Object/Array | Only for Object/Array | Nested field definitions. Required (at least 1 item) for complex types. Must be null or absent for primitives. |

### Field Types

Each field type maps to a category of business data. Choosing the right type ensures correct storage, validation, and UI rendering.

| Type | Category | Nested Fields | When to Use |
|---|---|---|---|
| `String` | Primitive | No | Names, labels, descriptions, emails, URLs, addresses, statuses, any free text |
| `Number` | Primitive | No | Quantities, amounts, prices, percentages, scores, numeric IDs |
| `Boolean` | Primitive | No | Yes/no flags, toggles, binary states (e.g., "Is Active," "Is Verified") |
| `DateTime` | Primitive | No | Dates, timestamps, deadlines, schedules |
| `File` | Primitive | No | Attachments, images, documents, uploads |
| `Identifier` | Primitive | No | References to other entity instances or external system IDs |
| `Object` | Complex | Yes (min 1) | Groups related sub-fields into a single unit (e.g., an address with street, city, zip) |
| `Array` | Complex | Yes (min 1) | Repeating structured items (e.g., line items, phone numbers with label and number) |

---

## Schema Name Generation

Schema names exist to give every entity and field a stable, machine-readable identifier that never changes. Display names are for humans and can be renamed freely. Schema names are for systems and must remain constant so that stored data, integrations, and queries never break.

### How Schema Names Are Generated

- Non-alphanumeric characters are treated as word separators
- First word is fully lowercased, subsequent words are capitalized (camelCase)

| Display Name | Generated Schema Name |
|---|---|
| Customer Order | `customerOrder` |
| First Name | `firstName` |
| My CRM Entity | `myCrmEntity` |

### Immutability Rules

- Schema names are assigned at creation and **never change**
- On update, fields with a matching `schemaName` preserve their identity
- Only newly added fields (without a `schemaName`) get auto-generated names
- Renaming a `displayName` does not change the `schemaName`

### Reserved Schema Names

These names are used by the platform internally and cannot be used as field schema names at any nesting depth:

`id`, `eTag`, `createdAt`, `updatedAt`, `isDeleted`, `tenantId`, `schemaName`, `type`

---

## Validation Rules and Limits

### Entity-Level Constraints

| Constraint | Value | Why |
|---|---|---|
| Display name length | 2–128 characters | Ensures meaningful but concise naming |
| Display name content | Must contain at least one alphanumeric character | Prevents empty or symbol-only names |
| Description length | Max 200 characters | Keeps descriptions brief and scannable |
| Top-level fields per entity | Max 100 | Prevents excessively wide schemas that degrade performance |
| Tags per entity | Max 20 | Encourages focused categorization |
| Tag length | Max 64 characters | Keeps tags concise |
| Tag format | `^[a-zA-Z0-9_-]+$` | Ensures tags are URL-safe and queryable |
| Schema name uniqueness | Must be unique within the tenant | Prevents ambiguous entity references |

### Field-Level Constraints

| Constraint | Value | Why |
|---|---|---|
| Display name length | 2–128 characters | Same reasoning as entity names |
| Nested fields per level | Max 50 | Prevents overly complex nested structures |
| Maximum nesting depth | 5 levels | Bounds recursion to keep data manageable |
| Schema name duplicates | Not allowed within the same nesting level | Ensures each field at a level is uniquely addressable |
| Schema name reserved words | Rejected at any depth | Prevents collisions with platform-internal properties |
| Object/Array fields | Must have at least 1 nested field | A complex type without sub-fields has no meaning |
| Primitive fields | Must not have nested fields | Primitives are leaf nodes by definition |

---

## Create Entity Definition

Defines a new type of business object within the tenant. The schema name is auto-generated from the display name (or can be provided explicitly). Once created, the schema name is immutable.

**Method:** `POST /blueprints/entity-definitions`

### Request Body

```json
{
  "displayName": "Customer Order",
  "description": "Represents a customer purchase order with line items and totals",
  "isAbstract": false,
  "extendsEntityId": null,
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

### Request Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `displayName` | string | Yes | 2–128 chars, must contain at least one alphanumeric character |
| `description` | string | No | Explains the business purpose of this entity type. Max 200 chars. |
| `isAbstract` | boolean | No | Defaults to `false`. Set to `true` for base types meant to be extended. |
| `extendsEntityId` | GUID | No | ID of parent entity definition to inherit from. Must exist in the same tenant. |
| `schemaName` | string | No | Custom override for the auto-generated schema name. Use only when you need a specific identifier. |
| `fields` | array | No | Field definitions describing the entity's data shape. Defaults to empty array. |
| `tags` | array | No | Classification labels. Defaults to empty array. |

### Responses

- **201 Created**: Returns full Entity Definition with `Location` header
- **400 Validation Failed**: Request violates constraints (see [Error Formats](#error-formats))
- **409 Conflict**: Schema name already exists in the tenant

### Business Rules

Before creation, the system enforces:
1. **Schema name uniqueness**: The generated (or provided) schema name must not already exist in the tenant. Rejected with 409 if duplicate.
2. **Parent entity validation**: If `extendsEntityId` is provided, the referenced entity must exist in the same tenant.

---

## Update Entity Definition

Modifies an existing entity type. Requires the current `eTag` to prevent conflicting overwrites. The `schemaName` of the entity itself cannot be changed.

**Method:** `PUT /blueprints/entity-definitions/{id}`

### Request Body

```json
{
  "eTag": 1,
  "displayName": "Customer Order v2",
  "description": "Updated description with new priority field",
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

### Request Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `eTag` | long | Yes | Must match the entity's current version. Prevents lost updates when multiple clients modify the same entity. |
| `displayName` | string | No | New human-readable name. Does not change the schema name. |
| `description` | string | No | Pass `null` to clear. |
| `isAbstract` | boolean | No | |
| `fields` | array | No | Full replacement of the field tree. See below. |
| `tags` | array | No | Full replacement of tags. |

### Field Update Behavior

The field tree is a **full replacement** with schema name preservation:
- Fields that include a `schemaName` matching an existing field keep that schema name (field identity is preserved)
- Fields without a `schemaName` are treated as new additions and get auto-generated schema names
- Fields present in the old tree but absent in the new tree are removed

This means: to rename a field's display name without breaking integrations, include its existing `schemaName` in the update payload.

### Responses

- **200 OK**: Returns updated Entity Definition
- **404 Not Found**: Entity does not exist
- **409 Conflict (eTag mismatch)**: Another client modified the entity since you last read it

---

## List Entity Definitions

Retrieves a paginated list of entity definitions within the tenant. Supports filtering by name, abstract status, and tags for discovery.

**Method:** `GET /blueprints/entity-definitions`

### Query Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `skip` | int | 0 | Number of items to skip (for pagination) |
| `take` | int | 20 | Page size, max 100 |
| `search` | string | — | Full-text search on display name |
| `isAbstract` | boolean | — | Filter to abstract-only or concrete-only entity types |
| `tags` | string | — | Comma-separated tag filter (e.g., `crm,billing`). Returns entities matching any of the listed tags. |

### Response

```json
{
  "items": [ /* array of Entity Definitions */ ],
  "totalCount": 42
}
```

---

## Get Entity Definition

Retrieves a single entity definition by its ID.

**Method:** `GET /blueprints/entity-definitions/{id}`

### Responses

- **200 OK**: Returns single Entity Definition
- **404 Not Found**: Entity does not exist in this tenant

---

## Delete Entity Definition

Permanently removes an entity definition from the tenant. This is irreversible.

**Method:** `DELETE /blueprints/entity-definitions/{id}`

### Responses

- **200 OK**: Entity permanently removed
- **404 Not Found**: Entity does not exist

---

## Error Formats

### Validation Error (400)

Returned when a request violates validation constraints. The `errors` object uses dot-path notation to identify exactly which property failed (e.g., `fields[0].type` for the first field's type, `fields[1].fields[0].displayName` for a nested field's name).

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

### Not Found (404)

Returned when the requested entity definition does not exist in the tenant.

```json
{
  "type": "https://api.dilcore.com/errors/not-found",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Entity definition '{id}' does not exist.",
  "errorCode": "NOT_FOUND"
}
```

### Conflict — Schema Name (409)

Returned when creating an entity whose schema name (generated or provided) already exists in the tenant. Each schema name must be unique per tenant.

### Conflict — ETag Mismatch (409)

Returned when updating an entity whose version has changed since the client last read it. The client must re-read the entity, get the new eTag, and retry.

```json
{
  "type": "https://api.dilcore.com/errors/etag-mismatch",
  "title": "Conflict",
  "status": 409,
  "detail": "ETag mismatch for entity definition '{id}'. Expected {current}, got {provided}.",
  "errorCode": "ETAG_MISMATCH"
}
```
