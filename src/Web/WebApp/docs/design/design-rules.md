# Dilcore Platform — design rules

Use this as the full system prompt for Stitch (or any design work). It describes **what the product should look and feel like**, not implementation.

---

## Product character

**Dilcore Platform** is a multi-tenant SaaS workspace: calm, enterprise-grade, and slightly “premium” — clear hierarchy, generous whitespace, and minimal visual noise. AI and admin surfaces should feel **capable** without looking gimmicky.

---

## Typography

- **Font family:** **Inter** (regular, medium, bold).
- **Body:** ~14px equivalent; comfortable line height for long reading.
- **Hierarchy:** Strong page titles; section titles medium weight; metadata and hints in **secondary** color — never compete with primary actions.
- **Page titles:** Large, confident; tight letter-spacing is fine for a modern SaaS look.
- **Supporting text:** Rely on **secondary** color, not only smaller size.
- **Buttons:** **Sentence case**, not all-caps.
- **Dark mode:** Primary text should feel **soft white** (`#FAFAFA`), not harsh pure white everywhere — reserve full white for high-emphasis moments.

---

## Spacing and layout grid

- Follow a **4px baseline**: **4, 8, 16, 24, 32, 48** px.
- **Cards and panels:** ~**24px** inner padding by default.
- **Between major sections:** **32–48px** vertical separation before adding more chrome.
- Prefer **whitespace** over extra borders; borders are **hairline** and subtle.

---

## Shapes

- **Default corner radius:** ~**12px** on cards, dialogs, and primary inputs — soft, not bubbly.
- **Pills and chips:** Fully rounded for small controls (filters, tags).

---

## Shell patterns (where to use which frame)

**Simple app shell** — Global flows after sign-in when there is no tenant workspace yet (e.g. choosing a workspace). Top bar with account; main column **centered and width-limited** on large monitors.

**Minimal shell** — Focused one-task flows (e.g. complete profile). Almost no chrome: content only.

**Workspace shell** — Day-to-day work **inside a tenant**: **persistent left navigation** (brand, primary destinations, account at the bottom), **fixed top bar** with **breadcrumbs**, **search**, **workspace vs admin** switch, **light/dark toggle**. Main area **full width** with consistent page padding — default “product” feel.

**Admin shell** — Same frame as workspace; navigation and labels reflect **administration** (settings, AI tools, blueprints-related areas). Search and section naming can sound more operational.

**Workspace / admin top bar (Stitch note)** — This shell may use a **dark glass** top bar: near-opaque dark with **blur** so content scrolls underneath. Keep **breadcrumb and control contrast** high (light text on dark bar). For a **fully light** variant of this shell, use a **light neutral top bar** and **dark text**.

---

## Information architecture (for naming frames)

- **Global** — Pre-workspace or cross-workspace (workspace picker, account).
- **Workspace** — `/workspaces/{tenant}` — primary tenant experience.
- **Admin** — `/workspaces/{tenant}/admin/...` — configuration, AI agent, blueprints-style areas.

Name screens consistently, e.g. **`[Area] — [Screen]`**.

---

## Component vocabulary (recurring building blocks)

- App bar, side drawer, breadcrumbs  
- Cards (often **flat**: outline + light fill, not heavy shadow)  
- Lists and data tables  
- Tabs, dialogs, text fields, selects  
- Chips and tags for roles or status  
- Primary vs text/secondary actions  
- Icon buttons for dense toolbars  
- **Empty states** with one clear primary action  
- **Loading:** inline progress or full-screen blocking state with a short message  
- **Errors:** short inline or toast-style messages — readable, not alarming unless destructive  

---

## Depth, cards, and elevation

- Default surfaces are **flat**: **1px border** and subtle fill instead of drop shadows.
- **Interactive cards** may lift slightly on hover (light shadow + stronger border) — clickable tiles only, not static panels.
- **Dialogs and modals** sit clearly above content (border + shadow acceptable).
- **Light mode:** Hairline borders (`#E4E4E7`); shadows mainly on hover or floating UI (dialogs, dropdowns). **Glass-style** centered panels: very light translucent surface + subtle blur for sign-in / onboarding.
- **Dark mode:** Hairline borders (`#27272A`); **interactive cards** may use a **soft primary-tinted glow** on hover — subtle, not neon.

---

## Feedback and trust

- Important actions need **immediate feedback** (loading, success, or error).
- **Errors** are concise and actionable; avoid technical jargon in user-facing copy.
- **Unauthorized** states should invite **sign-in** clearly.

---

## Anti-patterns

- Random accent colors outside the palettes below.
- Tight, uneven padding on the same screen type.
- Different navigation shells per feature without a reason.
- Heavy drop shadows everywhere — reads as dated, not premium.
- Cluttered forms — prefer **progressive disclosure** (sections, tabs, accordions).

---

## Light mode — palette

| Role | Hex | Usage |
|------|-----|--------|
| **Primary** | `#5565DD` | Primary buttons, links, brand accents, selected states |
| **Secondary** | `#414141` | Secondary emphasis |
| **Background** | `#FFFFFF` | Main page canvas |
| **Surface** | `#F9FAFB` | Cards, side panels, raised areas |
| **App bar (fill)** | `#F9FAFB` | Top bar background |
| **App bar (text)** | `#0F172A` | Top bar labels and icons (default) |
| **Drawer (fill)** | `#F9FAFB` | Left navigation background |
| **Drawer (text)** | `#0F172A` | Navigation labels |
| **Drawer (icons)** | `#64748B` | Navigation icons (muted) |
| **Text primary** | `#0F172A` | Headlines and body |
| **Text secondary** | `#64748B` | Descriptions, hints, timestamps |
| **Interactive default** | `#0F172A` | Default clickable text/icons |
| **Disabled (fg)** | `#E4E4E7` | Disabled labels |
| **Disabled (bg)** | `#F9FAFB` | Disabled surfaces |
| **Border / divider** | `#E4E4E7` | Rules, tables, inputs, cards |
| **Success** | `#10B981` | Positive outcomes |
| **Warning** | `#F59E0B` | Caution |
| **Error** | `#EF4444` | Errors, destructive emphasis |
| **Info** | `#3B82F6` | Informational accents |
| **Black / white** | `#000000` / `#FFFFFF` | Sparingly |

**Light mode mood:** Bright and **clean**, not stark — off-white surfaces (`#F9FAFB`) on white (`#FFFFFF`) for gentle layering without busy gradients.

---

## Dark mode — palette

| Role | Hex | Usage |
|------|-----|--------|
| **Primary** | `#5565DD` | Buttons, links, accents (same brand as light) |
| **Text on primary** | `#FFFFFF` | Label/icon on primary-filled controls |
| **Secondary** | `#414141` | Secondary emphasis |
| **Background** | `#09090B` | Main canvas (rich dark, not pure black) |
| **Surface** | `#18181B` | Cards, panels, elevated regions |
| **App bar (fill)** | `#18181B` | Top bar background |
| **App bar (text)** | `#FFFFFF` | Top bar foreground |
| **Drawer (fill)** | `#09090B` | Left navigation background |
| **Drawer (text)** | `#A1A1AA` | Navigation labels |
| **Drawer (icons)** | `#A1A1AA` | Navigation icons |
| **Text primary** | `#FAFAFA` | Headlines and body |
| **Text secondary** | `#A1A1AA` | Descriptions, hints, timestamps |
| **Interactive default** | `#A1A1AA` | Default clickable text/icons |
| **Disabled (fg)** | `#27272A` | Disabled foreground |
| **Disabled (bg)** | `#121212` | Disabled surfaces |
| **Border / divider** | `#27272A` | Rules, tables, inputs, cards |
| **Success** | `#10B981` | Positive outcomes |
| **Warning** | `#F59E0B` | Caution |
| **Error** | `#EF4444` | Errors, destructive emphasis |
| **Info** | `#3B82F6` | Informational accents |
| **Black / white** | `#000000` / `#FFFFFF` | Sparingly |

**Dark mode mood:** **Confident and calm** — deep greys and zinc tones, not empty black fields; primary purple stays the anchor for action and focus.
