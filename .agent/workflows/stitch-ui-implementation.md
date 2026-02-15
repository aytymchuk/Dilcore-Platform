---
description: Make UI changes based on a Stitch screen
---

1.  **Parse and Validate Input**:
    *   **Goal**: Ensure we have the necessary project and screen identifiers.
    *   **Action**: Check the user request for a Stitch Project ID and Screen ID, or a Stitch URL in the format `https://stitch.withgoogle.com/projects/{{Project ID}}?node-id={{Screen ID}}`.
    *   **If missing**: Ask the user to provide the Stitch URL or Project/Screen IDs.
    *   **If present**: Proceed to the next step.
    *   **Tip**: If you have a URL, extract the `{{Project ID}}` (the segment after `projects/`) and the `{{Screen ID}}` (the value of `node-id` query parameter).

2.  **Analyze Stitch Screen**:
    *   **Goal**: Understand the visual design and structure of the target screen.
    *   **Tool**: `stitch_get_screen`
        *   `projectId`: Extracted Project ID
        *   `screenId`: Extracted Screen ID
        *   `name`: `projects/{{Project ID}}/screens/{{Screen ID}}`
    *   **Action**: carefully examine the returned screen data. Note the layout structure, specific components (buttons, cards, inputs), typography, colors, and spacing.
    *   **Output**: Summarize the key design elements and identify any potential challenges or custom implementations needed.

3.  **Scan Codebase for Reusable Assets**:
    *   **Goal**: Identify existing components and styles to reuse, avoiding duplication.
    *   **Tools**:
        *   `list_dir` `src/Web/WebApp/Components/Common` (and subdirectories like `Buttons`, `Cards`) to find reusable components (e.g., `EntityPrimaryButton`, `EntityCard`).
        *   `view_file` `src/Web/WebApp/wwwroot/app.css` (and/or `src/Web/WebApp/Components/Themes/FutureSlateTheme.cs`) to understand available global styles and theme variables.
    *   **Action**: Map the identified Stitch elements to existing Razor components and CSS classes.
    *   **Output**: A list of reusable components and styles that match the Stitch design.

4.  **Investigate MudBlazor Styles (Context7)**:
    *   **Goal**: Determine the best MudBlazor components and utility classes to achieve the desired layout and style.
    *   **Tools**:
        *   `mcp_context7_resolve-library-id` (if needed) with `libraryName="MudBlazor"`.
        *   `mcp_context7_query-docs` with queries like "MudBlazor card layout examples", "MudGrid responsive design", "MudButton variants", etc.
    *   **Action**: Research how to implement specific design patterns (e.g., shadowing, elevation, spacing) using MudBlazor's utility classes.
    *   **Output**: Recommended MudBlazor components and classes for the implementation.

5.  **Plan Razor Page Changes**:
    *   **Goal**: Create a step-by-step plan for modifying the Razor page.
    *   **Action**: based on the Stitch analysis and codebase scan, outline the changes for the target Razor page (`.razor`) and its code-behind (`.razor.cs`).
        *   Identify which standard HTML elements should be replaced with MudBlazor components.
        *   Specify which reusable components (from step 3) should be integrated.
        *   Define any new CSS classes or inline styles required (minimize if possible by using `app.css` or MudBlazor utilities).
    *   **Output**: A detailed implementation plan.

6.  **Apply Changes**:
    *   **Goal**: Implement the planned changes in the codebase.
    *   **Tools**: `replace_file_content` (or `multi_replace_file_content` for larger edits).
    *   **Action**: Execute the plan. Update the `.razor` file structure and logic. Update `.razor.cs` if needed. Add necessary `using` directives for components.
    *   **Verify**: Ensure the changes compile and syntactically make sense.

7.  **Verify and Refine**:
    *   **Goal**: Confirm the changes match the Stitch design.
    *   **Action**: If possible, run the app and verify the visual output. If not, carefully review the code against the design requirements and ask the user for feedback.
