# Blueprints Domain — Relationships

Relationships define how one entity definition connects to another. They answer the question: **"How do these business object types point to each other?"** This domain part lets tenants model cardinality (`OneToOne`, `OneToMany`, `ManyToOne`) and keep both sides of a link consistent, so navigation remains predictable in downstream consumers.

Relationships are attached to entity definitions and returned as part of the entity definition payload documented in [entities.md](entities.md).

## Base Path

`/blueprints/entity-definitions`

All endpoints require authentication and the `x-tenant` header.

## Operations

| Operation | Method | Path | Description |
|---|---|---|---|
| Add Relationship | `POST` | `/blueprints/entity-definitions/{id}/references` | Add a relationship from one entity definition to another |
| Remove Relationship | `DELETE` | `/blueprints/entity-definitions/{id}/references/{schemaName}` | Remove a relationship by schema name from the source entity |

---

## Relationship Structure

A relationship describes one directed link from a source entity definition to a target entity definition. The platform automatically maintains a reverse link on the target side.

### Relationship Object (Response Format)

```json
{
  "schemaName": "orderCustomer",
  "referenceType": "ManyToOne",
  "relatedEntityDefinitionId": "2f4e1e5a-0c9c-4a3d-bfbe-1348fbe66984",
  "relatedEntitySchemaName": "customer"
}
```

### Property Reference

| Property | Type | Description |
|---|---|---|
| `schemaName` | string | Stable identifier of this relationship on the source entity definition. If omitted on create, the system generates one from source and target schema names. |
| `referenceType` | string | Cardinality from source to target. Allowed values: `OneToOne`, `OneToMany`, `ManyToOne`. |
| `relatedEntityDefinitionId` | GUID | ID of the target entity definition in the same tenant. |
| `relatedEntitySchemaName` | string | Schema name of the target entity definition. Helps consumers reason about relationship intent. |

### Reference Type Semantics

| Source Type | Meaning | Auto-Created Reverse Type |
|---|---|---|
| `OneToOne` | One source maps to one target | `OneToOne` |
| `OneToMany` | One source maps to many targets | `ManyToOne` |
| `ManyToOne` | Many sources map to one target | `OneToMany` |

---

## Validation Rules and Limits

### Relationship-Level Constraints

| Constraint | Value | Why |
|---|---|---|
| Reference types | `OneToOne`, `OneToMany`, `ManyToOne` | Keeps relationship semantics explicit and bounded |
| Related entity id | Required, non-empty GUID | Prevents orphaned links |
| Relationship schema name length | Max 64 characters | Keeps identifiers predictable and storage-safe |
| Relationships per entity definition | Max 50 | Prevents excessive graph density on one entity |
| Relationship schema name format | camelCase, lowercase start, alphanumeric only | Preserves stable machine-readable identifiers |
| Relationship schema name uniqueness (per source entity) | Must be unique, case-insensitive | Prevents ambiguous relationship targeting |

### Generated Schema Name Behavior

When `schemaName` is not provided:
- The platform combines source and target entity schema names into a camelCase relationship name
- If the name already exists on the source entity, numeric suffixes (`2`, `3`, ...) are appended until unique
- If no valid unique name can be produced, the request is rejected

---

## Add Relationship

Creates a directed relationship from the source entity definition (`{id}`) to the target entity definition (`relatedEntityDefinitionId`) and automatically creates the inverse relationship on the target entity definition.

### Request Body

```json
{
  "schemaName": "orderCustomer",
  "referenceType": "ManyToOne",
  "relatedEntityDefinitionId": "2f4e1e5a-0c9c-4a3d-bfbe-1348fbe66984"
}
```

### Request Properties

| Property | Type | Required | Description |
|---|---|---|---|
| `schemaName` | string | No | Optional explicit relationship name. If omitted, generated automatically. |
| `referenceType` | string | Yes | Source-to-target cardinality (`OneToOne`, `OneToMany`, `ManyToOne`). |
| `relatedEntityDefinitionId` | GUID | Yes | Target entity definition id. |

### Responses

- **201 Created**: Returns the updated source entity definition including its `references`
- **400 Validation Failed**: Invalid reference type, invalid schema name, missing target id, or other relationship validation failure
- **404 Not Found**: Source entity definition does not exist

### Business Rules

- The target entity definition must exist in the same tenant.
- A reverse relationship is created automatically on the target entity definition using inverse cardinality.
- Self-relationships are supported and create a paired reverse entry on the same entity definition.
- If reverse-link creation fails, the operation is compensated so partial links are not left behind.

---

## Remove Relationship

Removes a relationship from the source entity definition by `schemaName` and removes the reverse relationship from the target entity definition when present.

### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `id` | GUID | Source entity definition id |
| `schemaName` | string | Relationship schema name on the source entity definition |

### Responses

- **200 OK**: Relationship removed from source and reverse side (when present)
- **404 Not Found**: Source entity definition not found or relationship schema name not found on that entity
- **400 Validation Failed**: Reverse-side removal fails with a validation error

### Business Rules

- `schemaName` matching is case-insensitive.
- The `schemaName` path value is normalized to the canonical schema naming format before lookup.
- Reverse removal ignores missing reverse links to support resilient cleanup.

---

## Error Formats

### Validation Error (400)

```json
{
  "type": "https://api.dilcore.com/errors/data-validation-failed",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errorCode": "DATA_VALIDATION_FAILED",
  "errors": {
    "referenceType": ["ReferenceType must be OneToOne, OneToMany, or ManyToOne."]
  }
}
```

Occurs when the relationship payload or schema constraints are invalid.

### Not Found (404)

```json
{
  "type": "https://api.dilcore.com/errors/not-found",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Reference with schema name 'orderCustomer' does not exist on this entity.",
  "errorCode": "NOT_FOUND"
}
```

Occurs when the source entity definition or the relationship being removed cannot be found.
