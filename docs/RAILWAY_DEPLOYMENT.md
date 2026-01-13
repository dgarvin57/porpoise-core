# Railway Deployment Configuration

## Summary

Successfully deployed Porpoise to Railway with separate production and staging environments:

1. **Fixed Production Deployment Issues**
   - Resolved CORS configuration to allow cross-origin requests
   - Fixed Vite environment variable handling by adding `ARG` declarations in Dockerfile
   - Configured proper root directories (empty for API, `porpoise-ui` for UI)

2. **Established Two-Environment Structure**
   - Production environment deploys from `main` branch
   - Staging environment deploys from `develop` branch
   - Each environment has its own isolated MySQL database

3. **Resolved Service Naming Issues**
   - Railway requires unique service names across all environments in a project
   - Used project-level environments (dropdown at top) rather than service-level environments
   - Each environment contains: `porpoise-api`, `pulse-ui`, and a separate database service

4. **Database Setup**
   - Created separate databases for production (`pulse-db-production`) and staging (`pulse-db-staging`)
   - Copied production data to staging for realistic testing
   - Configured connection strings using Railway's variable references for automatic credential management

## Railway Project Structure

**Project Name:** pulse-porpoise

**Environments:**
- `production` - deploys from `main` branch
- `staging` - deploys from `develop` branch

## Production Environment

### Services

#### porpoise-api
- **Type:** GitHub deployment
- **Repository:** dgarvin57/porpoise-core
- **Branch:** `main`
- **Root Directory:** (empty - needs access to sibling projects)
- **Public URL:** `https://porpoise-api-production.up.railway.app`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Production
RAILWAY_MAX_REQUEST_SIZE=100MB
ConnectionStrings__PorpoiseDb=Server=${{pulse-db-production.RAILWAY_PRIVATE_DOMAIN}};Port=3306;Database=railway;User=root;Password=${{pulse-db-production.MYSQL_ROOT_PASSWORD}};
```

#### pulse-ui
- **Type:** GitHub deployment
- **Repository:** dgarvin57/porpoise-core
- **Branch:** `main`
- **Root Directory:** `porpoise-ui`
- **Public URL:** `https://pulse-ui-production.up.railway.app`

**Environment Variables:**
```
VITE_API_BASE_URL=https://porpoise-api-production.up.railway.app
```

#### pulse-db-production
- **Type:** MySQL database
- **Database Name:** `railway`

**Connection Details:**
- **Host:** `tramway.proxy.rlwy.net`
- **Port:** `16979`
- **User:** `root`
- **Password:** (see Railway dashboard)
- **Database:** `railway`

**Connection String:** Available in Railway under `pulse-db-production` → Variables → `MYSQL_PUBLIC_URL`

## Staging Environment

### Services

#### porpoise-api
- **Type:** GitHub deployment
- **Repository:** dgarvin57/porpoise-core
- **Branch:** `develop`
- **Root Directory:** (empty - needs access to sibling projects)
- **Public URL:** `https://porpoise-api-staging.up.railway.app`

**Environment Variables:**
```
ASPNETCORE_ENVIRONMENT=Staging
RAILWAY_MAX_REQUEST_SIZE=100MB
ConnectionStrings__PorpoiseDb=Server=${{pulse-db-staging.RAILWAY_PRIVATE_DOMAIN}};Port=3306;Database=railway;User=root;Password=${{pulse-db-staging.MYSQL_ROOT_PASSWORD}};
```

#### pulse-ui
- **Type:** GitHub deployment
- **Repository:** dgarvin57/porpoise-core
- **Branch:** `develop`
- **Root Directory:** `porpoise-ui`
- **Public URL:** `https://pulse-ui-staging.up.railway.app`

**Environment Variables:**
```
VITE_API_BASE_URL=https://porpoise-api-staging.up.railway.app
```

#### pulse-db-staging
- **Type:** MySQL database
- **Database Name:** `railway`

**Connection Details:**
- **Host:** `interchange.proxy.rlwy.net`
- **Port:** `55255`
- **User:** `root`
- **Password:** (see Railway dashboard)
- **Database:** `railway`

**Connection String:** Available in Railway under `pulse-db-staging` → Variables → `MYSQL_PUBLIC_URL`

## Deployment Workflow

### Development → Staging
1. Make changes on `develop` branch
2. Commit and push to GitHub
3. Railway automatically deploys to staging environment
4. Test at `https://pulse-ui-staging.up.railway.app`

### Staging → Production
1. Merge `develop` branch to `main`
2. Push to GitHub
3. Railway automatically deploys to production environment
4. Live at `https://pulse-ui-production.up.railway.app`

## Important Configuration Notes

### Root Directory Settings
- **API services:** Leave root directory **empty** (not `/` or `.`)
  - Reason: The API project references sibling projects (Porpoise.Core, Porpoise.DataAccess) and needs access to the repository root to restore dependencies
- **UI services:** Set to `porpoise-ui`
  - Reason: Contains the Vue/Vite application with its own package.json

### Vite Environment Variables
- Must be prefixed with `VITE_`
- Are baked into the build at **build time**, not runtime
- Require service redeploy when changed
- Must be declared as `ARG` in Dockerfile to be accessible during build:
  ```dockerfile
  ARG VITE_API_BASE_URL
  ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
  RUN npm run build
  ```

### Database Connection Strings
- Use Railway's variable references: `${{service-name.VARIABLE_NAME}}`
- Example: `${{pulse-db-production.MYSQL_ROOT_PASSWORD}}`
- Automatically updates if database credentials change
- No need to manually update connection strings

### CORS Configuration
- Temporarily set to `AllowAnyOrigin()` for Railway deployment
- Located in `Porpoise.Api/Program.cs`
- TODO: Restrict to specific origins once domains are finalized

## GitHub Actions

Workflows have been simplified to run tests only. Railway handles deployment via auto-deploy from GitHub.

**Workflow Files:**
- `.github/workflows/deploy-api.yml` - Runs API tests on push to `main`
- `.github/workflows/deploy-ui.yml` - Runs UI tests on push to `main`

**Test Execution:**
- Unit tests run in CI/CD
- Integration tests (DataAccess.Tests) excluded from workflows (require MySQL)

## Database Management

### Copying Production to Staging
```bash
# Get connection details from Railway dashboard variables
# Export from production
mysqldump -h <PROD_HOST> -P <PROD_PORT> -u root -p<PROD_PASSWORD> railway > production_backup.sql

# Import to staging
mysql -h <STAGING_HOST> -P <STAGING_PORT> -u root -p<STAGING_PASSWORD> railway < production_backup.sql
```

Use `MYSQL_PUBLIC_URL` from each database service in Railway for connection details.

### Accessing Databases via TablePlus
Use the connection details listed above for each environment.

## Troubleshooting

### UI not connecting to API
1. Verify `VITE_API_BASE_URL` is set correctly
2. Redeploy the UI service (Vite bakes variables at build time)
3. Hard refresh browser (Cmd+Shift+R / Ctrl+Shift+R)

### API connection errors
1. Check database connection string in service variables
2. Verify database service is running
3. Check Railway logs for specific error messages

### Build failures
1. Check Railway deployment logs
2. Verify root directory settings
3. Ensure all environment variables are set

### Shared database between environments
If you copy an environment, databases may be shared initially:
1. Create a new database service in the target environment
2. Update connection strings to point to the new database
3. Remove the shared database service from the target environment (doesn't delete it from source)

## Cost Optimization

Railway pricing is usage-based. To optimize costs:
- Use staging for testing, production for live traffic
- Both databases are running 24/7 (minimal cost for low usage)
- Consider pausing staging services when not actively developing
- Monitor usage in Railway dashboard

## URLs Reference

### Production
- **UI:** https://pulse-ui-production.up.railway.app
- **API:** https://porpoise-api-production.up.railway.app
- **API Test:** https://porpoise-api-production.up.railway.app/api/projects

### Staging
- **UI:** https://pulse-ui-staging.up.railway.app
- **API:** https://porpoise-api-staging.up.railway.app
- **API Test:** https://porpoise-api-staging.up.railway.app/api/projects

## Important Configuration Notes

### File Upload Size Limits

Railway's proxy has a **10MB default limit** for HTTP request bodies. To support larger file uploads (e.g., .porpz survey archives), you **must** add the following environment variable to the API service:

```
RAILWAY_MAX_REQUEST_SIZE=100MB
```

This is configured in both production and staging environments. Without this, file uploads over 10MB will fail with a `413 Payload Too Large` error.

**Note:** The ASP.NET application is also configured to support 100MB uploads via:
- `Kestrel.Limits.MaxRequestBodySize`
- `FormOptions.MultipartBodyLengthLimit`
- `[RequestSizeLimit]` attributes on upload endpoints

Both Railway's proxy limit AND the application limits must be set appropriately.

## Related Documentation

- **UI Deployment Configuration:** See `porpoise-ui/DEPLOYMENT.md` for details on:
  - Vite environment variable configuration
  - API URL configuration
  - Local vs production builds
  - Environment file structure

- **Security Configuration:** See `docs/SECURITY.md` for:
  - CORS settings
  - Rate limiting
  - Authentication requirements (future)
  - Security checklist for production
