---
trigger: glob
---

# Branch Management Context

## 1. Golden Rule

**Before ANY code change**: Run `git branch --show-current`.

- **On `main`**: Create a new branch immediately.
- **On another branch?** Verify it matches the user's intent. If not, ask to create a new one.

## 2. Naming Standards

- **Pattern**: `feature/<name>` or `fix/<name>`
- **Style**: Lowercase, kebab-case, concise (2-5 words).
- **Integrations**: Include issue ID if provided (e.g., `feature/issue-42-auth`, `fix/jira-99-crash`).

## 3. Workflow Sequence

When creating a branch:

```bash
# 1. Secure current state
git stash push -m "WIP"

# 2. Sync with source
git checkout main
git pull origin main

# 3. Create branch
git checkout -b <type>/<descriptive-name>

# 4. Restore work
git stash pop
```

## 4. Automation Safety

- **Safe (AutoRun=true)**: `git status`, `git branch`, `git log`, `git diff` (Read-only)
- **Unsafe (AutoRun=false)**: Any command that modifies state (checkout, pull, stash, commit, push).
