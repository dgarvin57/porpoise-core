# API Configuration - Deployment Guide

## Overview
All hardcoded API URLs have been replaced with environment-based configuration to support deployment to Railway or other platforms.

## Files Created

### Environment Files
- `.env.development` - Local development (http://localhost:5107)
- `.env.production` - Production placeholder (update with Railway URL)
- `.env.example` - Template for new developers

### Configuration Module
- `src/config/api.js` - Central API configuration
  - Exports `API_BASE_URL` constant
  - Exports `getApiUrl()` helper function
  - Falls back to localhost if env var not set

## Changes Made

### Files Updated (17 total)
- ✅ ProjectGallery.vue (10 URLs replaced)
- ✅ ResultsView.vue (2 URLs replaced)
- ✅ CrosstabView.vue (2 URLs replaced)
- ✅ AnalyticsView.vue (4 URLs replaced)
- ✅ TrashView.vue (1 URL replaced)
- ✅ ProjectCard.vue (2 URLs replaced)
- ✅ ProjectSettingsModal.vue (2 URLs replaced)
- ✅ DataView.vue (1 URL replaced)
- ✅ QuestionListSelector.vue (1 URL replaced)
- ✅ QuestionsView.vue (1 URL replaced)
- ✅ ProjectListItem.vue (1 URL replaced)
- ✅ SurveyEditModal.vue (1 URL replaced)
- ✅ OrganizationSettings.vue (2 URLs replaced)
- ✅ ProjectList.vue (1 URL replaced)
- ✅ ImportView.vue (3 URLs replaced)

### Configuration Updates
- ✅ vite.config.js - Added @ path alias support

## Railway Deployment Steps

### 1. Set Environment Variable in Railway
```bash
VITE_API_BASE_URL=https://your-api-url.up.railway.app
```

### 2. Update `.env.production` (optional)
```bash
VITE_API_BASE_URL=https://your-api-url.up.railway.app
```

### 3. Deploy
Railway will automatically use the environment variable during build.

## Local Development

No changes needed! The app will continue to use `http://localhost:5107` by default.

## Verification

Build succeeded with no errors:
```bash
npm run build
✓ 311 modules transformed
✓ built in 1.08s
```

All hardcoded URLs removed (except the fallback in config/api.js).

## Usage in Code

```javascript
// Import the config
import { API_BASE_URL } from '@/config/api'

// Use in API calls
const response = await axios.get(`${API_BASE_URL}/api/projects`)
```

## Environment Variable Format

**Variable Name**: `VITE_API_BASE_URL`
**Format**: URL without trailing slash
**Examples**:
- Development: `http://localhost:5107`
- Production: `https://api.myapp.com`
- Railway: `https://myapp-api.up.railway.app`
