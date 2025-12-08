# Pulse Deployment Guide

## Overview
Pulse is built on the Porpoise engine with:
- **Backend**: Porpoise API (.NET 10)
- **Frontend**: Pulse UI (Vue 3 + Vite)
- **Database**: MySQL

## Railway Project Structure

**Project Name**: Pulse

**Services**:
1. `porpoise-api` (staging environment)
2. `porpoise-api` (production environment)
3. `pulse-ui` (staging environment)
4. `pulse-ui` (production environment)
5. `pulse-db-staging` (MySQL)
6. `pulse-db-production` (MySQL)

## Step-by-Step Setup

### 1. Create Railway Project

1. Go to [Railway Dashboard](https://railway.app/dashboard)
2. Click **"New Project"**
3. Name it **"Pulse"**

### 2. Add MySQL Databases

**Staging Database:**
1. In Pulse project, click **"+ New"** → **"Database"** → **"MySQL"**
2. Name it: `pulse-db-staging`
3. Once created, go to **Variables** tab and copy `DATABASE_URL`
4. Save for later: This is `DATABASE_URL_STAGING`

**Production Database:**
1. Click **"+ New"** → **"Database"** → **"MySQL"**
2. Name it: `pulse-db-production`
3. Copy `DATABASE_URL` from Variables tab
4. Save for later: This is `DATABASE_URL_PRODUCTION`

### 3. Add API Services

**Staging API:**
1. Click **"+ New"** → **"GitHub Repo"**
2. Select: `porpoise-core` repository
3. Name the service: `porpoise-api`
4. Go to **Settings**:
   - **Environment**: Create "staging"
   - **Branch**: Set to `develop`
   - **Root Directory**: Set to `Porpoise.Api`
5. Go to **Variables** tab, add:
   ```
   DATABASE_URL = <paste staging database URL>
   ASPNETCORE_ENVIRONMENT = Staging
   PORT = 5000
   ```
6. Generate a Railway token:
   - Go to **Settings** → **Tokens** → **Create Token**
   - Name: "Staging API Token"
   - Save token as `RAILWAY_API_STAGING_TOKEN` (for GitHub secrets)

**Production API:**
1. In the same `porpoise-api` service, go to **Deployments**
2. Click settings icon → **"Add Environment"**
3. Create "production" environment
4. Go to **Settings**:
   - Switch to **production** environment
   - **Branch**: Set to `main`
   - **Root Directory**: `Porpoise.Api`
5. Go to **Variables** tab (production), add:
   ```
   DATABASE_URL = <paste production database URL>
   ASPNETCORE_ENVIRONMENT = Production
   PORT = 5000
   ```
6. Generate production token:
   - **Settings** → **Tokens** → **Create Token**
   - Name: "Production API Token"
   - Save as `RAILWAY_API_PRODUCTION_TOKEN`

### 4. Add UI Services

**Staging UI:**
1. Click **"+ New"** → **"GitHub Repo"**
2. Select: `porpoise-core` repository
3. Name the service: `pulse-ui`
4. Go to **Settings**:
   - **Environment**: Create "staging"
   - **Branch**: Set to `develop`
   - **Root Directory**: Set to `porpoise-ui`
5. Go to **Variables** tab, add:
   ```
   VITE_API_URL = <your staging API URL>
   PORT = 3000
   ```
6. Generate token:
   - **Settings** → **Tokens** → **Create Token**
   - Name: "Staging UI Token"
   - Save as `RAILWAY_UI_STAGING_TOKEN`

**Production UI:**
1. In the same `pulse-ui` service, add "production" environment
2. Go to **Settings**:
   - Switch to **production** environment
   - **Branch**: Set to `main`
   - **Root Directory**: `porpoise-ui`
3. Go to **Variables** tab (production), add:
   ```
   VITE_API_URL = <your production API URL>
   PORT = 3000
   ```
4. Generate token:
   - **Settings** → **Tokens** → **Create Token**
   - Name: "Production UI Token"
   - Save as `RAILWAY_UI_PRODUCTION_TOKEN`

### 5. Configure GitHub Secrets

Go to your GitHub repository: **Settings** → **Secrets and variables** → **Actions**

Add these secrets:
- `RAILWAY_API_STAGING_TOKEN` = (from step 3)
- `RAILWAY_API_PRODUCTION_TOKEN` = (from step 3)
- `RAILWAY_UI_STAGING_TOKEN` = (from step 4)
- `RAILWAY_UI_PRODUCTION_TOKEN` = (from step 4)
- `VITE_API_URL_STAGING` = (staging API URL from Railway)
- `VITE_API_URL_PRODUCTION` = (production API URL from Railway)
- `DATABASE_URL_STAGING` = (from step 2)
- `DATABASE_URL_PRODUCTION` = (from step 2)

### 6. Migrate Local Database to Railway

**Export your local MySQL database:**
```bash
mysqldump -u root -p porpoise_db > porpoise_backup.sql
```

**Import to Railway Staging:**
1. Get staging database connection details from Railway
2. Import:
```bash
mysql -h <staging-host> -P <port> -u <user> -p<password> <database-name> < porpoise_backup.sql
```

**Test staging thoroughly, then import to production:**
```bash
mysql -h <production-host> -P <port> -u <user> -p<password> <database-name> < porpoise_backup.sql
```

### 7. Test Deployment

**Test Staging:**
1. Push to `develop` branch
2. GitHub Actions will automatically deploy
3. Check Railway logs for errors
4. Visit staging URL to test

**Deploy to Production:**
1. Create PR: `develop` → `main`
2. Review and merge
3. GitHub Actions will deploy to production
4. Visit production URL

## Workflow Summary

**Daily Development:**
```bash
# Work on develop branch
git checkout develop
# Make changes
git add .
git commit -m "feat: add new feature"
git push origin develop
# Auto-deploys to staging
```

**Promote to Production:**
1. Create PR on GitHub: `develop` → `main`
2. Review changes
3. Merge PR
4. Auto-deploys to production

## Troubleshooting

**Database Connection Issues:**
- Verify `DATABASE_URL` in Railway variables
- Check if database service is running
- Ensure connection string format is correct

**Build Failures:**
- Check GitHub Actions logs
- Verify Railway tokens are correct
- Check .NET SDK version (10.0)
- Check Node version (20)

**API Not Responding:**
- Check Railway logs: Service → Deployments → View Logs
- Verify `PORT` environment variable
- Check `ASPNETCORE_ENVIRONMENT` setting

**UI Build Issues:**
- Verify `VITE_API_URL` is set correctly
- Check that API is deployed first
- Ensure all npm packages installed

## Database Schema Updates

When you make database changes:

1. Update your local database
2. Export updated schema:
```bash
mysqldump -u root -p --no-data porpoise_db > schema.sql
```
3. Apply to staging database
4. Test thoroughly
5. Apply to production database

## Useful Commands

**Check Railway deployment status:**
```bash
railway status
```

**View Railway logs locally:**
```bash
railway logs
```

**Link local project to Railway:**
```bash
railway link
```

**Run migrations (if using EF Core):**
```bash
dotnet ef database update --project Porpoise.Api
```
