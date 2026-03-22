---
name: stitch-mudblazor-ui
description: Implements Blazor UI in src/Web/WebApp from Google Stitch designs via Stitch MCP, mapping Tailwind-heavy HTML to MudBlazor components and scoped CSS. Uses Context7 MCP for MudBlazor APIs. Use when the user shares a Stitch screen URL, asks to match a Stitch mockup, or to build admin/tenant UI aligned with a Stitch export.
---

# Stitch → MudBlazor UI (WebApp)

## Goal

Reproduce the **visual layout, hierarchy, spacing, typography, and colors** of the Stitch screen in the Dilcore Blazor **WebApp** using **MudBlazor** and project theming. Stitch exports are HTML with **Tailwind-style utility classes** — they are a **reference**, not something to paste verbatim into Razor.

## Prerequisites

- **Stitch MCP** (`user-stitch`): `get_screen`, and optionally `list_screens` / `list_projects`.
- **Context7 MCP** (`user-context7`): `resolve-library-id` then `query-docs` for MudBlazor (see limits below).
- Always read MCP tool schemas under the Cursor MCP descriptors folder before calling tools.

## Workflow

1. **Resolve the screen**
   - From a Stitch URL, extract `projectId` and screen id (`node-id`). See [reference.md](reference.md) for URL patterns.
   - Call **`get_screen`** with `name` = `projects/{projectId}/screens/{screenId}` (full resource name). If the URL only identifies the project, use **`list_screens`** with `projectId` and match by title or thumbnail.
   - Keep **`screenshot`** (e.g. `downloadUrl`) and **`htmlCode`** (structure + Tailwind) as the source of truth for fidelity.

2. **Plan the Razor structure**
   - Infer regions: app bar, drawers, grids, cards, lists, forms, dialogs, tabs, data tables, chips, avatars, etc.
   - **Prefer MudBlazor components** (`MudPaper`, `MudGrid`, `MudStack`, `MudText`, `MudButton`, `MudIconButton`, `MudTextField`, `MudSelect`, `MudTable`, `MudTabs`, `MudDialog`, `MudChip`, `MudAvatar`, `MudDivider`, `MudList`, …) instead of raw `<div>` forests unless Mud has no equivalent.

3. **Learn MudBlazor APIs (Context7)**
   - Call **`resolve-library-id`** with `libraryName` `MudBlazor` (and a short `query` describing the UI task).
   - Call **`query-docs`** with the chosen `libraryId` — **batch related questions** into one query when possible. Context7 guidance: avoid more than **three** `query-docs` calls per task; plan queries ahead (e.g. “MudGrid breakpoints + MudStack spacing + MudPaper elevation for a two-column admin layout”).

4. **Map Tailwind → implementation**
   - Do **not** add Tailwind to the WebApp for Stitch parity unless the project already standardizes on it (it does not).
   - Translate spacing (`p-*`, `m-*`, `gap-*`, `space-*`) → `MudGrid` / `MudStack` / `MudItem` spacing props or **scoped** `.razor.css` with `rem` values.
   - Translate colors (`bg-*`, `text-*`, borders) → **`MudTheme` / palette CSS variables** (e.g. `var(--mud-palette-*)`) or `color-mix()` for translucency, consistent with [FutureSlateTheme.cs](../../../src/Web/WebApp/Components/Themes/FutureSlateTheme.cs).
   - Translate flex/grid → Mud layout primitives first; use custom CSS only where Mud cannot express the design.

5. **Implement in WebApp**
   - Place features under `src/Web/WebApp/Features/…` and shared pieces under `src/Web/WebApp/Components/…`, following existing routes and layouts (`TenantWorkspaceShell`, `MainLayout`, etc.).
   - Use **`@using MudBlazor`** (already in `_Imports.razor` where appropriate).
   - Put non-trivial styling in **component-scoped** `*.razor.css` (see existing agent/chat panels for examples).

6. **Verify fidelity**
   - Compare running UI to the Stitch **screenshot**: sections, alignment, density, header/footer treatment, and typography hierarchy.
   - Fix gaps with scoped CSS or Mud props before adding bespoke components.

## Project conventions (must follow)

- Theme: layouts use **`FutureSlateTheme.Default`** — extend the theme only when the Stitch design requires recurring tokens (prefer theme over one-off magic colors).
- Reuse existing WebApp patterns: `EntityPrimaryButton`, `CenteredCard`, workspace shell, breadcrumb builder, etc., when they fit.
- Obey [.NET coding standards](../../rules/dotnet-coding-standards.mdc) and the **dilcore-architecture** skill for layer and feature placement.
- New behavior needs tests per repo rules; pure markup/CSS-only changes follow existing test expectations for the feature.

## Anti-patterns

- Embedding Stitch HTML wholesale or relying on Tailwind CDN in the Blazor app.
- Replacing MudBlazor with hand-built controls when a Mud component exists.
- Ignoring the screenshot and only approximating from memory.
- Spreading unrelated styling across global CSS when scoped CSS is enough.

## Extra detail

- Stitch MCP field formats, URL parsing, and a **Tailwind → Mud/CSS** cheat sheet: [reference.md](reference.md).
