#!/bin/bash

# Automated Version Bumping Script
# Single source of truth: porpoise-ui/package.json
# All other version files are generated or synced from this

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Get the root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}🚀 Porpoise Version Bumper${NC}"
echo -e "${YELLOW}Single source of truth: porpoise-ui/package.json${NC}"
echo

# Check if we're on a clean branch
if [[ -n $(git status --porcelain) ]]; then
    echo -e "${RED}❌ Working directory is not clean. Commit or stash changes first.${NC}"
    exit 1
fi

# Get current version from package.json (SINGLE SOURCE OF TRUTH)
CURRENT_VERSION=$(grep -o '"version": "[^"]*' "$ROOT_DIR/porpoise-ui/package.json" | grep -o '[0-9].*')
echo -e "Current version: ${YELLOW}$CURRENT_VERSION${NC}"

# Parse version components
IFS='.' read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"

# Get commits since last tag (or all if no tags)
LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
if [[ -z "$LAST_TAG" ]]; then
    echo "No previous tags found. Analyzing all commits..."
    COMMITS=$(git log --pretty=format:"%s" --no-merges)
else
    echo "Last tag: $LAST_TAG"
    COMMITS=$(git log "$LAST_TAG"..HEAD --pretty=format:"%s" --no-merges)
fi

# Determine bump type
BUMP_TYPE="none"

if echo "$COMMITS" | grep -q "BREAKING CHANGE:"; then
    BUMP_TYPE="major"
    echo -e "${RED}🔥 Found BREAKING CHANGE - will bump MAJOR version${NC}"
elif echo "$COMMITS" | grep -q "^feat:"; then
    BUMP_TYPE="minor"
    echo -e "${GREEN}✨ Found feat: - will bump MINOR version${NC}"
elif echo "$COMMITS" | grep -q "^fix:"; then
    BUMP_TYPE="patch"
    echo -e "${YELLOW}🔧 Found fix: - will bump PATCH version${NC}"
else
    echo -e "${YELLOW}⚠️  No conventional commits found (feat:, fix:, BREAKING CHANGE:)${NC}"
    echo "Recent commits:"
    echo "$COMMITS" | head -5
    echo
    echo "Would you like to manually bump? (major/minor/patch/none)"
    read -r MANUAL_BUMP
    if [[ "$MANUAL_BUMP" =~ ^(major|minor|patch)$ ]]; then
        BUMP_TYPE="$MANUAL_BUMP"
    else
        echo -e "${BLUE}ℹ️  No version bump. Exiting.${NC}"
        exit 0
    fi
fi

# Calculate new version
case "$BUMP_TYPE" in
    major)
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        ;;
    minor)
        MINOR=$((MINOR + 1))
        PATCH=0
        ;;
    patch)
        PATCH=$((PATCH + 1))
        ;;
    none)
        echo "No version bump needed"
        exit 0
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
echo
echo -e "${GREEN}📦 Bumping version: ${YELLOW}$CURRENT_VERSION${GREEN} → ${YELLOW}$NEW_VERSION${NC}"
echo

# Confirm
echo "Proceed with version bump? (y/n)"
read -r CONFIRM
if [[ "$CONFIRM" != "y" ]]; then
    echo "Aborted."
    exit 0
fi

echo -e "${BLUE}Updating versions...${NC}"

# 1. Update package.json (SOURCE OF TRUTH)
echo "  📝 package.json (source of truth)"
sed -i.bak "s/\"version\": \"$CURRENT_VERSION\"/\"version\": \"$NEW_VERSION\"/" "$ROOT_DIR/porpoise-ui/package.json"
rm "$ROOT_DIR/porpoise-ui/package.json.bak"

# 2. Update Porpoise.Api.csproj (synced from package.json)
echo "  📝 Porpoise.Api.csproj (synced)"
sed -i.bak "s/<Version>$CURRENT_VERSION<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
sed -i.bak "s/<AssemblyVersion>$CURRENT_VERSION.0<\/AssemblyVersion>/<AssemblyVersion>$NEW_VERSION.0<\/AssemblyVersion>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
sed -i.bak "s/<FileVersion>$CURRENT_VERSION.0<\/FileVersion>/<FileVersion>$NEW_VERSION.0<\/FileVersion>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
rm "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj.bak"

# 3. Generate version.ts (auto-generated from package.json)
echo "  📝 version.ts (auto-generated)"
cd "$ROOT_DIR/porpoise-ui" && node scripts/generate-version.js

# 4. Update VERSION.md
echo "  📝 VERSION.md (changelog)"
DATE=$(date +%Y-%m-%d)
TEMP_FILE=$(mktemp)

# Extract commit messages for changelog
CHANGELOG=""
if [[ -n "$LAST_TAG" ]]; then
    CHANGELOG=$(git log "$LAST_TAG"..HEAD --pretty=format:"- %s" --no-merges)
else
    CHANGELOG=$(git log --pretty=format:"- %s" --no-merges | head -10)
fi

# Create new version entry
cat > "$TEMP_FILE" << EOF
# Versioning Strategy

Porpoise uses **Semantic Versioning** (SemVer): \`MAJOR.MINOR.PATCH\`

## Version Format

- **MAJOR** (X.0.0): Breaking changes that require user action or migration
- **MINOR** (1.X.0): New features, enhancements, non-breaking changes  
- **PATCH** (1.0.X): Bug fixes, small tweaks, performance improvements

## Current Version: $NEW_VERSION

### Single Source of Truth
Version is maintained in \`porpoise-ui/package.json\` and auto-synced to:
- \`porpoise-ui/src/version.ts\` (generated during build)
- \`Porpoise.Api/Porpoise.Api.csproj\` (synced by bump script)

### Version History

#### $NEW_VERSION ($DATE)
$CHANGELOG

EOF

# Append the rest of VERSION.md (skip old current version section)
tail -n +13 "$ROOT_DIR/VERSION.md" >> "$TEMP_FILE"
mv "$TEMP_FILE" "$ROOT_DIR/VERSION.md"

# Stage changes
echo
echo "Staging changes..."
git add "$ROOT_DIR/porpoise-ui/package.json"
git add "$ROOT_DIR/porpoise-ui/src/version.ts"
git add "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
git add "$ROOT_DIR/VERSION.md"

# Create commit
echo "Creating commit..."
git commit -m "chore: bump version to $NEW_VERSION

Automated version bump based on conventional commits.
$BUMP_TYPE version bump: $CURRENT_VERSION → $NEW_VERSION

Source: porpoise-ui/package.json
Auto-synced to: API csproj and version.ts"

# Create tag
echo "Creating tag v$NEW_VERSION..."
git tag -a "v$NEW_VERSION" -m "Release v$NEW_VERSION"

echo
echo -e "${GREEN}✅ Version bumped successfully!${NC}"
echo -e "Current branch: ${YELLOW}$(git branch --show-current)${NC}"
echo
echo "Files updated:"
echo "  ✓ porpoise-ui/package.json (source of truth)"
echo "  ✓ porpoise-ui/src/version.ts (generated)"
echo "  ✓ Porpoise.Api/Porpoise.Api.csproj (synced)"
echo "  ✓ VERSION.md (changelog)"
echo
echo "Next steps:"
echo "  1. Review the changes: git show HEAD"
echo "  2. Push to remote: git push origin $(git branch --show-current)"
echo "  3. Push tag: git push origin v$NEW_VERSION"
echo
echo -e "${BLUE}🚀 Railway will auto-deploy when you push to main${NC}"


set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Get the root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}🚀 Porpoise Version Bumper${NC}"
echo

# Check if we're on a clean branch
if [[ -n $(git status --porcelain) ]]; then
    echo -e "${RED}❌ Working directory is not clean. Commit or stash changes first.${NC}"
    exit 1
fi

# Get current version from package.json
CURRENT_VERSION=$(grep -o '"version": "[^"]*' "$ROOT_DIR/porpoise-ui/package.json" | grep -o '[0-9].*')
echo -e "Current version: ${YELLOW}$CURRENT_VERSION${NC}"

# Parse version components
IFS='.' read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"

# Get commits since last tag (or all if no tags)
LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
if [[ -z "$LAST_TAG" ]]; then
    echo "No previous tags found. Analyzing all commits..."
    COMMITS=$(git log --pretty=format:"%s" --no-merges)
else
    echo "Last tag: $LAST_TAG"
    COMMITS=$(git log "$LAST_TAG"..HEAD --pretty=format:"%s" --no-merges)
fi

# Determine bump type
BUMP_TYPE="none"

if echo "$COMMITS" | grep -q "BREAKING CHANGE:"; then
    BUMP_TYPE="major"
    echo -e "${RED}🔥 Found BREAKING CHANGE - will bump MAJOR version${NC}"
elif echo "$COMMITS" | grep -q "^feat:"; then
    BUMP_TYPE="minor"
    echo -e "${GREEN}✨ Found feat: - will bump MINOR version${NC}"
elif echo "$COMMITS" | grep -q "^fix:"; then
    BUMP_TYPE="patch"
    echo -e "${YELLOW}🔧 Found fix: - will bump PATCH version${NC}"
else
    echo -e "${YELLOW}⚠️  No conventional commits found (feat:, fix:, BREAKING CHANGE:)${NC}"
    echo "Recent commits:"
    echo "$COMMITS" | head -5
    echo
    echo "Would you like to manually bump? (major/minor/patch/none)"
    read -r MANUAL_BUMP
    if [[ "$MANUAL_BUMP" =~ ^(major|minor|patch)$ ]]; then
        BUMP_TYPE="$MANUAL_BUMP"
    else
        echo -e "${BLUE}ℹ️  No version bump. Exiting.${NC}"
        exit 0
    fi
fi

# Calculate new version
case "$BUMP_TYPE" in
    major)
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        ;;
    minor)
        MINOR=$((MINOR + 1))
        PATCH=0
        ;;
    patch)
        PATCH=$((PATCH + 1))
        ;;
    none)
        echo "No version bump needed"
        exit 0
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
echo
echo -e "${GREEN}📦 Bumping version: ${YELLOW}$CURRENT_VERSION${GREEN} → ${YELLOW}$NEW_VERSION${NC}"
echo

# Confirm
echo "Proceed with version bump? (y/n)"
read -r CONFIRM
if [[ "$CONFIRM" != "y" ]]; then
    echo "Aborted."
    exit 0
fi

# Update package.json
echo "Updating porpoise-ui/package.json..."
sed -i.bak "s/\"version\": \"$CURRENT_VERSION\"/\"version\": \"$NEW_VERSION\"/" "$ROOT_DIR/porpoise-ui/package.json"
rm "$ROOT_DIR/porpoise-ui/package.json.bak"

# Update Porpoise.Api.csproj
echo "Updating Porpoise.Api/Porpoise.Api.csproj..."
sed -i.bak "s/<Version>$CURRENT_VERSION<\/Version>/<Version>$NEW_VERSION<\/Version>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
sed -i.bak "s/<AssemblyVersion>$CURRENT_VERSION.0<\/AssemblyVersion>/<AssemblyVersion>$NEW_VERSION.0<\/AssemblyVersion>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
sed -i.bak "s/<FileVersion>$CURRENT_VERSION.0<\/FileVersion>/<FileVersion>$NEW_VERSION.0<\/FileVersion>/" "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
rm "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj.bak"

# Update VERSION.md
echo "Updating VERSION.md..."
DATE=$(date +%Y-%m-%d)
TEMP_FILE=$(mktemp)

# Extract commit messages for changelog
CHANGELOG=""
if [[ -n "$LAST_TAG" ]]; then
    CHANGELOG=$(git log "$LAST_TAG"..HEAD --pretty=format:"- %s" --no-merges)
else
    CHANGELOG=$(git log --pretty=format:"- %s" --no-merges | head -10)
fi

# Create new version entry
cat > "$TEMP_FILE" << EOF
# Versioning Strategy

Porpoise uses **Semantic Versioning** (SemVer): \`MAJOR.MINOR.PATCH\`

## Version Format

- **MAJOR** (X.0.0): Breaking changes that require user action or migration
- **MINOR** (1.X.0): New features, enhancements, non-breaking changes  
- **PATCH** (1.0.X): Bug fixes, small tweaks, performance improvements

## Current Version: $NEW_VERSION

### Version History

#### $NEW_VERSION ($DATE)
$CHANGELOG

EOF

# Append the rest of VERSION.md (skip header and current version section)
tail -n +13 "$ROOT_DIR/VERSION.md" >> "$TEMP_FILE"
mv "$TEMP_FILE" "$ROOT_DIR/VERSION.md"

# Stage changes
echo
echo "Staging changes..."
git add "$ROOT_DIR/porpoise-ui/package.json"
git add "$ROOT_DIR/Porpoise.Api/Porpoise.Api.csproj"
git add "$ROOT_DIR/VERSION.md"

# Create commit
echo "Creating commit..."
git commit -m "chore: bump version to $NEW_VERSION

Automated version bump based on conventional commits.
$BUMP_TYPE version bump: $CURRENT_VERSION → $NEW_VERSION"

# Create tag
echo "Creating tag v$NEW_VERSION..."
git tag -a "v$NEW_VERSION" -m "Release v$NEW_VERSION"

echo
echo -e "${GREEN}✅ Version bumped successfully!${NC}"
echo -e "Current branch: ${YELLOW}$(git branch --show-current)${NC}"
echo
echo "Next steps:"
echo "  1. Review the changes: git show HEAD"
echo "  2. Push to remote: git push origin $(git branch --show-current)"
echo "  3. Push tag: git push origin v$NEW_VERSION"
echo
echo -e "${BLUE}🚀 Railway will auto-deploy when you push to main${NC}"
