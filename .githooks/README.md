# Git Hooks Installation

This directory contains git hooks to help maintain repository best practices.

## Installing Hooks

Run this command from the repository root:

```bash
chmod +x .githooks/pre-commit
cp .githooks/pre-commit .git/hooks/pre-commit
```

Or set git to use this hooks directory:

```bash
git config core.hooksPath .githooks
```

## Available Hooks

### pre-commit

Warns when attempting to commit version files that are managed by CI/CD:
- `package.json`
- `Porpoise.Api/Porpoise.Api.csproj`
- `VERSION.md`

The hook will prompt you before allowing the commit to proceed, helping prevent merge conflicts from manual version changes.
