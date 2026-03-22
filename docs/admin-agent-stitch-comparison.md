# Admin AI Agent: Stitch Design vs Implementation Comparison

**Stitch design:** [Admin AI Agent - Unified Content Update](https://stitch.withgoogle.com/projects/12076154935296123528?node-id=5d4e3ffbe2544b2eb17f1114411bbe0c)  
**Compared:** Implemented screen (Playwright MCP full-page screenshot) vs Stitch reference.

**Verification (Stitch MCP + Playwright MCP):**
- **Stitch MCP** `get_screen`: Design title "Admin AI Agent - Unified Content Update", 2560×2048, screenshot and HTML export URLs returned.
- **Playwright MCP**: App opened at `https://localhost:7200`, first tenant (e.g. yellow-corp) selected, navigated to `/workspaces/{tenant}/admin/agent`. Full-page screenshot saved as `admin-agent-implemented.png`. Accessibility snapshot confirms: Generated Schema + DRAFT + OrderSystem.json + copy, Blueprint Architect + Active & Ready + History/Settings, AI Assistant + user messages + input + Send + Attach Context + Parameters + "Press Enter to send".

---

## Screen vs actual visual state (difference check)

| Area | Stitch design | Actual visual state | Status |
|------|----------------|---------------------|--------|
| **Breadcrumb** | Administration / Blueprints / AI Agent | Administration / Dashboard (on agent page) | **Bug:** Layout did not re-render on route change; breadcrumb stayed at dashboard. Fixed by calling `StateHasChanged()` on every `LocationChanged` in `TenantLayoutBase` so breadcrumbs update when navigating to agent/entities/etc. |
| **Schema file header** | Dark stripe (`bg-background-dark/50`) | Dark stripe via `var(--mud-palette-background)` | Done (in `SchemaPreviewPanel.razor.css`) |
| **Chat header** | Subtle tint (`bg-background-dark/20`) | Subtle tint via `color-mix(… 20%, transparent)` | Done (in `ChatPanel.razor.css`) |
| **Chat input area** | Same subtle tint | Same tint | Done (in `ChatPanel.razor.css`) |
| **User message bubble** | `shadow-lg` | `box-shadow` applied | Done (in `ChatPanel.razor.css`) |
| **Message spacing** | `space-y-8` (2rem) | `mb-8` on message blocks | Done (in `ChatPanel.razor`) |
| **Main content width** | Full-bleed, ~2rem padding | Negative margin + 2rem padding in `.admin-agent-layout` | Done (in `AdminAgent.razor.css`) |
| **AI message body** | Inline code for e.g. `status`, `Customer` | Backtick-wrapped terms rendered as `<code class="ai-message-code">` (primary + monospace) | Done: `ChatPanel` parses backticks, sample messages use `` `status` ``, `` `Customer` ``, etc. |

All listed differences are now addressed.

---

## Summary

The implementation matches the Stitch layout and content structure (two-panel layout, Generated Schema left, Blueprint Architect chat right, same copy and messaging). The following changes bring the UI to a closer 1:1 match with the design.

---

## 1. Breadcrumbs

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| **Administration / Blueprints / AI Agent** | Administration / Dashboard / AI Agent (or Administration / AI Agent) | Show **Blueprints** between Administration and AI Agent. |

**Plan:** In `BreadcrumbBuilder`, when building admin breadcrumbs and the current page is a “blueprint” segment (`agent`, `entities`, `projections`, `forms`, `views`, `workflows`), insert a **Blueprints** breadcrumb between **Administration** and the current page label. Options: (a) add a non-linked “Blueprints” item, or (b) add a “Blueprints” segment with a generic href like `{adminRoot}` and keep “AI Agent” as the last item. Prefer (a) for minimal routing impact.

**File:** `src/Web/WebApp/Routing/BreadcrumbBuilder.cs`

---

## 2. Schema panel – file header background

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| File row: `bg-background-dark/50` (semi-transparent dark) | No background on `.schema-file-header` | Add a subtle background to the schema file header row. |

**Plan:** In `AdminAgent.razor.css`, add a background for `.schema-file-header` that matches the theme (e.g. surface/background with low opacity or a theme variable equivalent to `bg-background-dark/50`).

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor.css`

---

## 3. Chat header background

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| Chat header: `bg-background-dark/20` | No explicit background | Add a light tint so the header is visually distinct. |

**Plan:** In the same CSS file, add a background for `.chat-header` (e.g. `background: rgba(9, 9, 11, 0.2)` or a theme variable) so it aligns with the design’s `bg-background-dark/20`.

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor.css`

---

## 4. Chat input area background

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| Input strip: `bg-background-dark/20` | No background on `.chat-input-area` | Add the same subtle background as the chat header. |

**Plan:** Add a background for `.chat-input-area` consistent with the chat header (e.g. same rgba or theme variable).

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor.css`

---

## 5. User message bubble – shadow

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| User bubble: `shadow-lg` | No shadow on `.user-message` | Add a light shadow to the user message bubble. |

**Plan:** In `AdminAgent.razor.css`, add `box-shadow` to `.user-message` (e.g. MudBlazor elevation or a small `box-shadow`) to match `shadow-lg`.

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor.css`

---

## 6. AI message body – inline code

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| Inline `status`, `Customer` in `<code class="text-primary font-mono">` | Plain text only | Optionally render inline code for backtick-wrapped or marked-up terms in AI messages. |

**Plan:** Low priority for 1:1. Either: (a) keep plain text, or (b) add a small parser/renderer that wraps segments like `status` and `Customer` in a `<code>`-style span (e.g. primary color + monospace) when we add structured or markdown-like content later.

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor` (and optional small helper).

---

## 7. Message spacing

| Stitch design | Implementation | Change |
|---------------|----------------|--------|
| Messages: `space-y-8` (2rem) | Messages: `mb-6` (1.5rem) | Slightly increase vertical spacing between messages to match. |

**Plan:** In the Razor markup, change the bottom margin on message blocks from `mb-6` to `mb-8` (or equivalent) so spacing matches `space-y-8`.

**File:** `src/Web/WebApp/Features/Tenants/Admin/Agent/AdminAgent.razor`

---

## 8. Intentionally different (no change)

- **Sidebar branding:** Stitch uses “NucleusAI”; we use “DilcorePlatform” — product naming, no change.
- **Schema panel width:** Design `w-80` (320px) and our 320px match.
- **DRAFT chip, OrderSystem.json, copy button, JSON content:** Present and correct.
- **Blueprint Architect title, Active & Ready, History/Settings buttons:** Present and correct.
- **AI Assistant label + sparkle icon, user bubble (primary), timestamp, typing dots:** Present and correct.
- **Input placeholder, Send button, Attach Context, Parameters, “Press Enter to send”:** Present and correct.

---

## Implementation order

1. **Breadcrumbs** – `BreadcrumbBuilder.cs` (insert “Blueprints” for blueprint pages).
2. **CSS tweaks** – `.schema-file-header`, `.chat-header`, `.chat-input-area` backgrounds; `.user-message` shadow; message spacing in Razor (`mb-8`).
3. **Optional:** Inline code in AI messages when we have structured content.

After these changes, run the existing Playwright E2E tests and a quick visual check against the Stitch screenshot to confirm 1:1 alignment.
