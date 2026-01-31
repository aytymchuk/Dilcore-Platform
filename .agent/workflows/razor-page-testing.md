---
description: Comprehensive Razor page analysis and functional testing workflow with browser automation and UI/UX visual evaluation
---

# Razor Page Testing Workflow

Performs: UI/UX evaluation, backend API integration, validation testing, browser automation.

## Prerequisites
- WebApp running on `https://localhost:5042`
- User provides: Page name(s), optional GitHub issue URL

---

## Phase 0.5: Verify App Running (Required)

> **CRITICAL**: Verify app is accessible before testing.

### Steps
// turbo
```bash
dotnet run --project src/Web/WebApp/WebApp.csproj
```

1. Navigate to `https://localhost:5042/` via browser_subagent
2. Wait 2-3s for load, take screenshot
3. If error: check terminal, fix, retry

> **STOP**: Don't proceed until app loads successfully.

---

## Phase 0: GitHub Issue Integration (Optional)

If user provides issue URL, extract: owner, repo, issue_number

```
Tool: mcp_github-mcp-server_issue_read
- method: "get" / "get_comments"
- owner, repo, issue_number
```

Extract from issue: User Story, Acceptance Criteria, Technical Notes, Related Issues

---

## Phase 1: Page Discovery & Analysis

### 1.1 Locate Files
// turbo
```bash
find src/Web/WebApp -name "*.razor" -o -name "*.razor.cs" | grep -i "<pageName>"
```

### 1.2 Analyze Structure
**`.razor`**: Route path, layout, UI components, bindings, event handlers
**`.razor.cs`**: Injected services, state vars, lifecycle methods, validation, submit handlers

### 1.3 Trace Backend
1. Identify MediatR Commands/Queries
2. Find Command Handlers → API Client extensions → actual endpoints

### 1.4 Map Navigation
Check `RouteConstants.cs`, auth requirements, redirects

---

## Phase 2: Bug Identification

### UI Vulnerabilities
- Form validation edge cases (empty, too long, special chars)
- Loading/error state handling
- Disabled state management
- Responsive layout issues

### Backend Vulnerabilities
- API error handling (network failures, timeouts)
- Client/server validation mismatch
- Race conditions, missing CancellationToken
- Error message propagation

### Auth Issues
- Unauthenticated access, session expiration, token refresh

---

## Phase 2.5: UI/UX Visual Evaluation

> Reference: `.agent/skills/ui-ux/SKILL.md`

### Checklist
| Category | Check |
|----------|-------|
| **Colors** | Curated palette (not generic), off-white bg, rich dark greys for dark mode |
| **Typography** | Modern font (Inter/Roboto), clear hierarchy (H1>H2>Body), muted captions |
| **Spacing** | 4px grid, `pa-6` card padding, `my-8` section separation, breathing room |
| **Responsive** | `100dvh`, `overflow-y: auto`, mobile stacking, `Margin.Dense` inputs |
| **Components** | `Elevation="0" Outlined="true"` cards, `Variant.Filled Color.Primary` buttons |
| **Feedback** | Skeletons during load, Snackbar for messages, inline validation, button loading states |
| **Premium** | Professional polish, primary action highlight, empty states, visual feedback, glassmorphism |

---

## Phase 3: Test Case Generation

### Directory Structure
// turbo
```bash
mkdir -p test-cases/Features/<FeatureName>/<PageName>
```

### Categories
- **functional/**: Happy path, form validations, submit success/error
- **negative/**: Empty fields, max length, invalid formats
- **edge-cases/**: Boundary values, special chars, unicode, very long inputs
- **ui-ux/**: Visual compliance tests
- **issue-based/**: Acceptance criteria from GitHub issues

### Test Case Format
```markdown
# Test Case: [TC-XXX] Title
- **Page**: /path | **Category**: functional | **Priority**: P1
## Preconditions
## Steps
## Expected Results
## Status: [ ] Passed [ ] Failed
```

---

## Phase 4: Browser Testing

### Execution Flow
1. Open browser to app root
2. Navigate to target page
3. Execute test steps
4. Capture screenshots
5. Record results, mark pass/fail

### Commands
```
browser: navigate to https://localhost:5042/<route>
browser: type "<value>" in <field-selector>
browser: click <button-selector>
browser: verify <element> contains "<text>"
browser: screenshot as test_<id>_<step>
```

---

## Phase 5: Bug Fixing

For each bug:
1. **Document**: Steps to reproduce, expected vs actual, screenshot, root cause
2. **Fix**: Implement fix, ensure coding guidelines, add unit test
3. **Verify**: Re-run failed test, run regression tests

---

## Phase 6: Report Generation

Generate `test-cases/<PagePath>/SUMMARY.md`:
```markdown
# Test Summary: [Page Name]
- Total: X | Passed: Y | Failed: Z | Blocked: W

## Bug Summary
| ID | Description | Status | Fix Reference |

## Recommendations
```

---

## Appendix: Common Selectors
- Text fields: `input[id="<fieldName>"]`
- Buttons: `button:contains("<text>")`
- Errors: `.mud-input-error`
- Loading: `.mud-progress-circular`
