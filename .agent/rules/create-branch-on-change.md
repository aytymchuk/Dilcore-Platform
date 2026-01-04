---
trigger: always_on
description: Automatic branch management - check current branch and create new branches based on user context
---

# Branch Management Rule

## Overview
Before making any code changes, ALWAYS check the current branch. If on `main`, create a new branch based on the user request context.

## Branch Naming Patterns

### Feature Branches (new implementation or changes)
```
feature/short-descriptive-name
feature/issue-123-descriptive-name
feature/jira-456-descriptive-name
```

### Bug Fix Branches
```
fix/short-descriptive-name
fix/issue-789-bug-description
fix/ado-321-bug-description
```

### Naming Rules
- Use **lowercase** letters
- Use **hyphens** (`-`) for spacing between words
- Keep names **short and readable** (2-5 words max)
- Include **issue/ticket number** if provided by user (GitHub issue, Jira, ADO)
- Examples:
  - ✅ `feature/user-authentication`
  - ✅ `feature/issue-42-add-logging`
  - ✅ `fix/null-reference-error`
  - ✅ `fix/jira-789-memory-leak`
  - ❌ `feature/This_Is_A_Very_Long_Branch_Name`
  - ❌ `myFeature` (wrong pattern)

## Workflow Steps

### 1. Check Current Branch
```bash
git branch --show-current
```

### 2. If on `main` branch:

#### a. Check for Uncommitted Changes
```bash
git status --short
```

#### b. Stash Changes if Necessary
```bash
# If there are uncommitted changes
git stash push -m "WIP: stashing before branch creation"
```

#### c. Pull Latest Changes
```bash
git checkout main
git pull origin main
```

#### d. Determine Branch Name
- **If context is CLEAR**: Create branch with short, descriptive name based on user request
- **If context is UNCLEAR**: Ask user for clarification before proceeding
- **If issue/ticket link provided**: Extract number and include in branch name

#### e. Create and Checkout New Branch
```bash
# For features
git checkout -b feature/branch-name

# For bug fixes
git checkout -b fix/branch-name
```

#### f. Restore Stashed Changes (if any)
```bash
git stash pop
```

### 3. If NOT on `main` branch:

#### a. Analyze Current Branch Context
- Check branch name
- Review git log to understand current work
```bash
git log --oneline -5
git diff main...HEAD --stat
```

#### b. Assess User Request Context
- Compare current branch purpose with new user request
- Determine if new request fits current branch scope

#### c. Ask User for Confirmation
```
Current branch: {branch-name}
Current branch appears to be for: {inferred-purpose}

Your new request is for: {user-request-summary}

Options:
1. Continue on current branch (if closely related)
2. Create a new branch from main for this change
3. Switch to a different existing branch

Which would you prefer?
```

## Complete Git Command Sequence

### Scenario 1: On `main`, No Uncommitted Changes
```bash
git checkout main
git pull origin main
git checkout -b feature/new-feature-name
```

### Scenario 2: On `main`, With Uncommitted Changes
```bash
git stash push -m "WIP: stashing before branch creation"
git checkout main
git pull origin main
git checkout -b feature/new-feature-name
git stash pop
```

### Scenario 3: On `main`, With Issue Reference
```bash
# User provided: "Fix the login bug - GitHub Issue #42"
git stash push -m "WIP: stashing before branch creation"  # if needed
git checkout main
git pull origin main
git checkout -b fix/issue-42-login-bug
git stash pop  # if stashed
```

### Scenario 4: Not on `main`
```bash
# First check context
git branch --show-current
git log --oneline -5

# Then ask user before proceeding
# If user wants new branch:
git stash push -m "WIP: stashing current work"  # if needed
git checkout main
git pull origin main
git checkout -b feature/new-branch-name
# Optionally restore previous work
```

## Important Notes

1. **Always check branch first** - Run `git branch --show-current` before any changes
2. **Protect uncommitted work** - Use `git stash` to preserve uncommitted changes
3. **Stay updated** - Always `git pull origin main` before creating new branches
4. **Clear naming** - Branch names should be immediately understandable
5. **Issue tracking** - Include ticket numbers when provided
6. **Context validation** - When not on main, verify if current branch is appropriate for the new request

## Automation Flags

When running git commands through `run_command` tool:
- Set `SafeToAutoRun: true` for: `git status`, `git branch`, `git log`, `git diff` (read-only operations)
- Set `SafeToAutoRun: false` for: `git stash`, `git checkout`, `git pull`, `git checkout -b` (state-changing operations)