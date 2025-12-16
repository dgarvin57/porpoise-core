# Versioning Strategy

Porpoise uses **Semantic Versioning** (SemVer): `MAJOR.MINOR.PATCH`

## Version Format

- **MAJOR** (X.0.0): Breaking changes that require user action or migration
- **MINOR** (1.X.0): New features, enhancements, non-breaking changes  
- **PATCH** (1.0.X): Bug fixes, small tweaks, performance improvements

## Current Version: 1.3.1

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.3.1 (2025-12-16)
- fix: improve crosstab loading UX with skeleton and radio button selection

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.3.0 (2025-12-16)
- UI improvements: toggle button styling, question list formatting, and crosstab layout
- Remove debug console logging from StatSigView
- Refactor crosstab selection UX with Mode+Replace approach
- fix: clear promoted tab when switching to different survey
- feat: improve StatSig tab styling and state preservation
- refactor: reduce question list width and default sidebar to closed
- feat: improve light mode styling and UX
- Reduce font sizes and tab heights, refine More dropdown styling
- Add comprehensive test coverage for API controllers and services
- cleanup: remove all console logging from AI service
- cleanup: remove debug logging after fixing API key configuration
- debug: use ILogger for Railway-visible logs and simplify analyze-question logging
- debug: add console logging to track questionLabel issue
- chore: bump API version to 1.2.5 to force Railway rebuild with latest logging
- chore: bump UI version to 1.2.5 to force Railway rebuild
- debug: add more detailed request logging for AI endpoint

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.2.4 (2025-12-14)
- fix: add .env.production with API URL for production builds
- debug: log raw request body to see what UI sends in production

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.2.3 (2025-12-14)
- fix: add JSON property name mapping for AI analysis requests

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.2.2 (2025-12-14)
- fix: add detailed logging for AI analysis endpoint

### Single Source of Truth
Version is maintained in `porpoise-ui/package.json` and auto-synced to:
- `Porpoise.Api/Porpoise.Api.csproj` (synced by bump script)

### Version History

#### 1.2.1 (2025-12-14)
- fix: make bump-version.sh non-interactive in CI/CD
- fix: suppress obsolete BlkLabel/BlkStem warnings during migration
- ci: automate version bumping in CI/CD pipeline
- refactor: simplify version management to single source of truth

### Version History

#### 1.2.0 (2024-12-13)
- Added version display in About section
- Improved AI service error handling
- Fixed skeleton UI backgrounds
- Enhanced tab synchronization

#### 1.1.0 (Previous)
- UI improvements: sticky headers, dynamic height
- Analytics view enhancements
- Bug fixes

## How to Update Versions

Versions are now **automated** using conventional commits!

### Automated Versioning (Recommended)

1. **Make commits using conventional commit format:**
   - `feat: add new feature` → Bumps MINOR version (1.X.0)
   - `fix: correct bug` → Bumps PATCH version (1.0.X)
   - `BREAKING CHANGE: redesign API` → Bumps MAJOR version (X.0.0)

2. **Run the bump script:**
   ```bash
   ./scripts/bump-version.sh
   ```

3. **The script will:**
   - Analyze commits since last tag
   - Determine appropriate version bump
   - Update all version files automatically
   - Create a git commit and tag
   - Update VERSION.md with changelog

4. **Push to deploy:**
   ```bash
   git push origin develop
   git push origin --tags
   ```

### Manual Versioning (Fallback)

If you need to manually set a version:

### 1. API Version
Edit `Porpoise.Api/Porpoise.Api.csproj`:
```xml
<PropertyGroup>
  <Version>1.2.0</Version>
  <AssemblyVersion>1.2.0.0</AssemblyVersion>
  <FileVersion>1.2.0.0</FileVersion>
</PropertyGroup>
```

### 2. UI Version  
Edit `porpoise-ui/package.json`:
```json
{
  "version": "1.2.0"
}
```

Also update `porpoise-ui/src/views/PreferencesView.vue`:
```javascript
const uiVersion = ref('1.2.0')
```

## Commit Message Convention

While versions are manual, we use conventional commits for clarity:

- `feat:` New feature → Consider MINOR bump
- `fix:` Bug fix → Consider PATCH bump  
- `BREAKING CHANGE:` Breaking change → Requires MAJOR bump
- `chore:` Maintenance, dependencies → Usually PATCH
- `docs:` Documentation only → No version bump needed
- `refactor:` Code refactor without behavior change → PATCH

## Checking Deployed Versions

### In Development
- UI: Check browser console on app load
- API: `curl http://localhost:5107/version`

### In Production
- UI: Go to Preferences → About
- API: `curl https://porpoise-api-production.up.railway.app/version`

## Railway Deployment

Railway automatically deploys when you push to `main` branch. The version displayed will match what's in the csproj/package.json files at deploy time.

### Health Check
`https://porpoise-api-production.up.railway.app/health`

Returns:
```json
{
  "status": "healthy",
  "timestamp": "2024-12-13T...",
  "environment": "Production"
}
```

### Version Check  
`https://porpoise-api-production.up.railway.app/version`

Returns:
```json
{
  "version": "1.2.0",
  "environment": "Production", 
  "timestamp": "2024-12-13T..."
}
```

## Before Release Checklist

1. ✅ Update version in `Porpoise.Api/Porpoise.Api.csproj`
2. ✅ Update version in `porpoise-ui/package.json`
3. ✅ Update version in `porpoise-ui/src/views/PreferencesView.vue` (uiVersion)
4. ✅ Update this VERSION.md with release notes
5. ✅ Test locally
6. ✅ Commit with message: `chore: bump version to X.Y.Z`
7. ✅ Push to develop
8. ✅ Merge to main
9. ✅ Verify deployment on Railway
10. ✅ Check `/version` endpoint matches new version
