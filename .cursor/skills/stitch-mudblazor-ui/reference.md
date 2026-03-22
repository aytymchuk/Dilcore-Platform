# Stitch + Tailwind reference for WebApp implementation

## Stitch URLs

Typical pattern:

`https://stitch.withgoogle.com/projects/{projectId}?node-id={screenId}`

- **`projectId`**: numeric string (e.g. `12076154935296123528`).
- **`screenId`**: hex-like string from `node-id` (e.g. `5d4e3ffbe2544b2eb17f1114411bbe0c`).

If the user pastes a different Stitch path, extract the project and screen identifiers the same way: **project** from `/projects/{id}`, **screen** from `node-id` query parameter when present.

## Stitch MCP: `get_screen`

- **Tool**: `get_screen` on server `user-stitch` (see MCP descriptor for exact argument shape).
- **Resource name**: `name` = `projects/{projectId}/screens/{screenId}` (single string including both IDs).
- **Response**: use `title`, `deviceType`, `width` / `height`, `screenshot` (`downloadUrl` for image), and `htmlCode` for exported HTML (often Tailwind-oriented classes).

If `get_screen` fails (wrong id), try **`list_screens`** with `projectId` and pick the screen whose `title` or metadata matches the design the user described.

## Tailwind → MudBlazor / CSS (practical mapping)

| Stitch / Tailwind idea | Prefer in WebApp |
|------------------------|------------------|
| `flex flex-col gap-*` | `MudStack` (`Spacing`, `Row`/`Column`) |
| `flex flex-row`, toolbar | `MudStack Row="true"` or `MudToolBar` |
| Grid columns | `MudGrid` + `MudItem xs="12" md="6"` (match breakpoints to design) |
| Card surfaces | `MudPaper` (`Elevation`, `Class`) |
| Primary / text / surfaces | `MudText`, `Color="Primary"`, surfaces via theme or `Class` + palette vars |
| Buttons / icon buttons | `MudButton`, `MudIconButton`, `MudFab` as appropriate |
| Inputs, selects | `MudTextField`, `MudSelect`, `MudCheckBox`, `MudSwitch`, `MudSlider`, … |
| Lists | `MudList`, `MudListItem`, or `MudTable` for tabular data |
| Chips, badges | `MudChip`, `MudBadge` |
| Dividers | `MudDivider` |
| Dialogs / drawers | `MudDialog`, `MudDrawer` (match Stitch modals vs side panels) |
| `rounded-*` | `BorderRadius` on theme, `MudPaper` `Square="false"`, or scoped `border-radius` |
| `shadow-*` | `Elevation` on `MudPaper` / `MudCard`, or `box-shadow` in scoped CSS |
| `bg-…/opacity` | `color-mix(in srgb, var(--mud-palette-*) N%, transparent)` or rgba |
| Fixed header/footer areas | Existing shell/layout; use `MudAppBar`, `MudMainContent`, or feature-level flex + `min-height: 0` for scroll regions (match admin agent patterns) |

## MudBlazor documentation (Context7)

1. `resolve-library-id` — `libraryName`: `MudBlazor`, `query`: short description of the UI (e.g. “data table with sorting and toolbar”).
2. `query-docs` — include component names and desired behavior in one query when possible to stay within the ~3-call guidance.

## Fidelity checklist

- [ ] Overall column/layout widths vs Stitch screenshot  
- [ ] Vertical rhythm (margins between blocks vs `space-y-*` / `gap-*`)  
- [ ] Header bars and subtle backgrounds (`bg-*` tints → palette + opacity)  
- [ ] Typography scale (headline vs body vs caption)  
- [ ] Interactive controls recognizable and aligned (inputs, buttons, icons)  
- [ ] Dark/light behavior matches design if design is mode-specific  
