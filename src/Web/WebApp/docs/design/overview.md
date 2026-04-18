# Dilcore Platform — design system overview

This describes **what the product should look and feel like**, not how it is built.

---

## Product character

**Dilcore Platform** is a multi-tenant SaaS workspace: calm, enterprise-grade, and slightly “premium” — clear hierarchy, generous whitespace, and minimal visual noise. AI and admin surfaces should feel **capable** without looking gimmicky.

---

## Typography

- **Font family:** **Inter** (regular, medium, bold). 
- **Body:** ~14px equivalent; comfortable line height for long reading.
- **Hierarchy:** Strong page titles; section titles medium weight; metadata and hints in **secondary** color, smaller or lighter — never compete with primary actions.
- **Buttons:** **Sentence case**, not all-caps.

---

## Spacing and layout grid

- Follow a **4px baseline**: spacing and padding resolve to **4, 8, 16, 24, 32, 48** px mentally.
- **Cards and panels:** ~**24px** inner padding as the default “breathing room.”
- **Between major sections:** use **32–48px** vertical separation before adding more chrome.
- Prefer **whitespace** over extra borders; borders are **hairline** and subtle.

---

## Shell patterns (where to use which frame)

**Simple app shell** — For global flows after sign-in when there is no tenant workspace yet (e.g. choosing a workspace). Top bar with account; main column **centered and width-limited** so content does not span edge-to-edge on large monitors.

**Minimal shell** — For focused one-task flows (e.g. complete profile). Almost no chrome: content only, full attention on the task.

**Workspace shell** — For day-to-day work **inside a tenant**: **persistent left navigation** (brand, primary destinations, account at the bottom), **fixed top bar** with **breadcrumbs**, **search**, a **workspace vs admin** switch, and **light/dark toggle**. Main area is **full width** with consistent page padding — this is the default “product” feel.

**Admin shell** — Same frame as workspace, but navigation and labels reflect **administration** (settings, AI tools, blueprints-related areas as the product grows). Search placeholder and section naming can sound more “global” or operational.

---

## Information architecture (for naming frames)

- **Global** — Pre-workspace or cross-workspace (workspace picker, account).
- **Workspace** — `/workspaces/{tenant}` — primary tenant experience.
- **Admin** — `/workspaces/{tenant}/admin/...` — configuration, AI agent, blueprints-style areas.

Name screens consistently, e.g. **`[Area] — [Screen]`**.

---

## Component vocabulary (recurring building blocks)

Design with these so screens stay consistent:

- App bar, side drawer, breadcrumbs  
- Cards (often **flat**: outline + light fill, not heavy shadow)  
- Lists and data tables  
- Tabs, dialogs, text fields, selects  
- Chips and tags for roles or status  
- Primary button vs text/secondary actions  
- Icon buttons for dense toolbars  
- **Empty states** with one clear primary action  
- **Loading:** inline progress or full-screen blocking state with a short message  
- **Errors:** short inline or toast-style messages — readable, not alarming unless destructive  

---

## Depth, cards, and elevation

- Default surfaces favor a **flat** look: **1px border** and subtle fill instead of drop shadows.
- **Interactive cards** may lift slightly on hover (light shadow + stronger border) — use for clickable tiles, not for static panels.
- **Dialogs and modals** sit above content with clear separation (border + shadow acceptable).

---

## Feedback and trust

- Every important action should have **immediate feedback** (loading, success, or error).
- **Errors** are concise and actionable; avoid long technical jargon in user-facing copy.
- **Unauthorized** states should invite **sign-in** clearly.

---

## Anti-patterns

- Random accent colors outside the approved light and dark palettes.
- Tight, uneven padding on the same screen type.
- Different navigation shells for each feature without a documented reason.
- Heavy drop shadows on everything — reads as dated, not premium.
- Cluttered forms — prefer **progressive disclosure** (sections, tabs, accordions).
