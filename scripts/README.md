# Porpoise Scripts

## Version Management

### `bump-version.sh`

Automated version bumping based on conventional commits.

**Usage:**
```bash
./scripts/bump-version.sh
```

**What it does:**
1. Analyzes commits since last tag
2. Determines version bump type based on conventional commits:
   - `feat:` → MINOR bump (1.X.0)
   - `fix:` → PATCH bump (1.0.X)
   - `BREAKING CHANGE:` → MAJOR bump (X.0.0)
3. Updates version in:
   - `porpoise-ui/package.json`
   - `porpoise-ui/src/views/PreferencesView.vue`
   - `Porpoise.Api/Porpoise.Api.csproj` (3 places)
   - `VERSION.md` (with changelog)
4. Creates git commit and tag
5. Provides push instructions

**Requirements:**
- Clean working directory (no uncommitted changes)
- Git repository with commit history

**Example:**
```bash
# After making commits with conventional format
git commit -m "feat: add crosstab analysis"
git commit -m "fix: correct spelling in error message"

# Run version bump
./scripts/bump-version.sh

# Output:
# Current version: 1.2.0
# ✨ Found feat: - will bump MINOR version
# 📦 Bumping version: 1.2.0 → 1.3.0
# ✅ Version bumped successfully!

# Push changes
git push origin develop
git push origin --tags
```

**Manual Bump:**
If no conventional commits found, script will ask:
```
Would you like to manually bump? (major/minor/patch/none)
```

**Rollback:**
If you need to undo a version bump before pushing:
```bash
git reset --hard HEAD~1
git tag -d v1.3.0
```

## Conventional Commit Format

Follow this format for automatic version bumping:

```
<type>: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat:` New feature (MINOR bump)
- `fix:` Bug fix (PATCH bump)
- `docs:` Documentation only (no bump)
- `style:` Formatting, missing semicolons (no bump)
- `refactor:` Code refactor (PATCH bump)
- `test:` Adding tests (no bump)
- `chore:` Maintenance (no bump)

**Breaking Changes:**
Add `BREAKING CHANGE:` in footer or use `!` after type:
```
feat!: redesign authentication API

BREAKING CHANGE: authentication now requires OAuth2
```

**Examples:**
```bash
git commit -m "feat: add AI analysis modal"
git commit -m "fix: correct crosstab calculation"
git commit -m "docs: update API documentation"
git commit -m "feat!: change survey data structure"
```
