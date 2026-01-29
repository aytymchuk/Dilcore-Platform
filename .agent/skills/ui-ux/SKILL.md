---
name: ui-ux
description: Guidelines for building modern, premium UI/UX for the MudBlazor-based NoCode Platform.
---

# UI/UX Skill: Modern MudBlazor Design

This skill provides guidelines and best practices for creating a "Premium", "Modern", and "AI-Driven" user experience using the MudBlazor framework for the SaaS NoCode Platform.

## 🌟 Core Design Philosophy

1.  **Premium First**: Every screen must look professional and polished. Avoid default "Bootstrap" looks.
2.  **AI-Driven Aesthetics**: Use sleek interfaces, subtle gradients, glassmorphism, and smooth animations to convey intelligence and modernity.
3.  **Simplicity**: Complex NoCode features must be presented simply. Use progressive disclosure.
4.  **Consistency**: Adhere strictly to the defined Design System tokens and spacing rules.

## 🎨 Design System & Theming

### Color Palette Strategy
Avoid generic RGB colors. Use a curated palette that feels "Enterprise SaaS".

-   **Primary**: Use a strong, defining brand color (e.g., deep violet, electric blue, or sophisticated teal).
-   **Background**: Avoid pure white (`#FFFFFF`) for large areas if possible; prefer slightly off-white or very light cool grays (`#F8F9FA`) to reduce eye strain and add richness.
-   **Dark Mode**: Essential for "AI" vibes. Use rich dark greys (`#121212` or `#1E1E2E`) rather than pure black.

### Typography

-   Use a modern sans-serif font (e.g., **Inter**, **Roboto**, or **Public Sans**).
-   Use distinct font weights to establish hierarchy (Bold headers, regular body, light captions).
-   **Text Hierarchy**:
    -   `H1`: Page Titles (Light/Regular weight, large).
    -   `H2/H3`: Section Headers (Medium/Bold).
    -   `Subtitle1`: Lead text or descriptions.
    -   `Body1`: Default text (14px-16px).
    -   `Caption`: Metadata, timestamps (use `Color="Color.Text.Secondary"`).

---

## 📐 Spacing & Layout System (White Space Strategy)

We follow a strict **4px baseline grid**. All margins and paddings must be multiples of 4.

### 1. Spacing Tokens (MudBlazor Helpers)

| Token | Pixels | Usage |
| :--- | :--- | :--- |
| `1` | 4px | Tight separation (e.g., icon from text) |
| `2` | 8px | Default component internal spacing |
| `4` | 16px | Standard separation between related elements |
| `6` | 24px | Section padding (Standard Card Padding) |
| `8` | 32px | Major component separation |
| `12` | 48px | Page section separation |

### 2. Layout Rules

-   **Card Padding**: ALWAYS use `Class="pa-6"` (24px) for standard content cards. Never use default padding if it feels tight.
-   **Section Separation**: Use `Class="my-8"` or `Class="py-8"` to separate major logical blocks.
-   **Grid Gaps**:
    -   Property panels/Forms: `Spacing="2"` or `Spacing="3"` (compact).
    -   Dashboard widgets: `Spacing="4"` (standard).
-   **Container Margins**: Main content should have breathing room. Use `MudContainer` with `Class="mt-4 mb-8"` minimum.

### 3. "The Breath Test"
*If a UI looks cluttered, DOUBLE the whitespace.*
-   Don't rely on lines/borders to separate content. Use **Whitespace** first.

---

## 📱 Fluid & Responsive Strategy

### 1. Viewport Handling
-   **Full Height**: Use `height: 100dvh` (Dynamic Viewport Height) instead of `100vh` for full-screen layouts. This handles mobile address bars correctly.
-   **Overflow**: ALWAYS prefer `overflow-y: auto` over `overflow: hidden` for full-screen containers. This ensures users on landscape mobile or small screens can still reach content.
-   **Centering**: For centered layouts, ensure the parent container (like `MudLayout` or `MudMainContent`) does not have default padding (`pt-0`) unless an AppBar is present, to ensure direct mathematical centering.

### 2. Mobile-First Forms

-   **Stacking**: "One Field Per Row". Avoid splitting simple fields into multi-column grids (e.g., First Name / Last Name side-by-side) on mobile. Use full width for better readability and input ease.
-   **Size Strategy**:
    -   Use `Size.Medium` for standard actions. Reserve `Size.Large` for very specific marketing/landing page heroes only.
    -   Use `Margin="Margin.Dense"` on `MudTextField` to keep forms compact and professional.

### 3. Minimalist Input Design
-   **No Redundant Icons**: Avoid putting icons *inside* text fields (`Adornment`) unless they provide critical functional context (e.g., search, visibility toggle). Labels should be sufficient.
-   **Cleanliness**: Reduce visual noise. Let the typography and spacing define the form.

---

## 🚀 Advanced SaaS & AI Patterns

### 1. The "Canvas" Metaphor (NoCode Builders)
For builder interfaces (flow charts, page editors), break the standard document flow.
-   **Remove the containers**: Use the full viewport height/width.
-   **Floating Controls**: Place toolbars and palettes as **Overlay** elements (absolute positioning with high Z-index) rather than fixed sidebars.
-   **Infinite Workspace**: Visual cue (dots/grid background) to imply an infinite canvas.

### 2. AI Prompt Interfaces
AI interactions should feel special, not just another form field.
-   **Centrality**: Place the prompt input centrally or in a dedicated "Command" modal (Cmd+K style).
-   **Visual Weight**: Use a glow effect or gradient border on the active prompt field.
-   **Streaming Response**: ALWAYS show a streaming indicator (typing effect) or "thinking" state. Never leave the user guessing.

### 3. Glassmorphism & Depth
Use depth to indicate hierarchy in complex apps.
- **Base Layer**: Serves as the primary background or canvas.
- **Context (Layer 1)**: Sidebars and navigation systems (Solid/Opaque).
- **Content (Layer 2)**: Main information displays like Cards and Tables (Solid/Opaque).
- **Overlays (Layer 3)**: Modals, Floating Toolbars, and AI Toasts.
    -   *Style*: `backdrop-filter: blur(8px); background: rgba(255,255,255,0.8);` (Adjust for dark mode).

### 4. Progressive Disclosure
Don't overwhelm the user with all options at once.
-   **Accordion/Expansion Panels**: For advanced settings.
-   **Tabs**: To categorize configuration (e.g., "General", "Data", "Design").
-   **"More" Menus**: Hide secondary actions behind a `MudMenu` (dots icon).

---

## 🛠 MudBlazor Implementation Best Practices

### 1. Root Configuration
Ensure `MainLayout` wraps content correctly and `MudThemeProvider` is configured with the custom theme.

```razor
<MudThemeProvider Theme="@_currentTheme" />
<MudDialogProvider />
<MudSnackbarProvider />
```

### 2. Component Guidelines

-   **Cards (`MudPaper`)**:
    -   Use `Elevation="0"` + `Outlined="true"` for a clean, modern look.
    -   Use `Class="pa-6"` (24px) for content padding.
-   **Inputs (`MudTextField`, `MudSelect`)**:
    -   **Do not hardcode `Variant`**. Rely on `MudThemeProvider` defaults to ensure global consistency.
    -   Only use explicit variants if a specific deviation is required for a unique UI context.
-   **Buttons (`MudButton`)**:
    -   Primary: `Variant="Variant.Filled" Color="Color.Primary"`.
    -   Secondary: `Variant="Variant.Text"`.
    -   **Icon Buttons**: Use heavily for dense interfaces (`MudIconButton`).
-   **Data Display (`MudDataGrid`)**:
    -   `Elevation="0"`, `Outlined="true"`.
    -   `Hover="true"`, `Striped="false"`.
    -   **Density**: Provide a density toggle for power users.

### 3. Feedback & Loading
-   **Skeletons**: Use `MudSkeleton` for layout stability during load.
-   **Toast**: `MudSnackbar` for success/info.
-   **Inline Validation**: Real-time form validation messages.

---

## ✅ UX QA Checklist

- [ ] **Spacing**: Are standard margins (16px/24px) used consistently?
- [ ] **Hierarchy**: Is the primary action properly highlighted vs. secondary actions?
- [ ] **Data Density**: Is the screen too crowded? Can we use whitespace or progressive disclosure?
- [ ] **Empty States**: Do empty lists/tables have a friendly illustration and "Create" CTA?
- [ ] **Feedback**: Does every click provide immediate visual feedback (ripple, loader, transition)?
