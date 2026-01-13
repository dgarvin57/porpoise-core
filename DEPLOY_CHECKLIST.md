# Railway Deployment Checklist

## 🚨 Critical: File Upload Configuration

**BEFORE** deploying changes that involve file uploads, ensure the following environment variable is set in Railway:

### Production Environment
1. Go to Railway Dashboard → pulse-porpoise → **production** environment
2. Select `porpoise-api` service
3. Go to **Variables** tab
4. Add or verify:
   ```
   RAILWAY_MAX_REQUEST_SIZE=100MB
   ```
5. Deploy/restart the service

### Staging Environment
1. Go to Railway Dashboard → pulse-porpoise → **staging** environment
2. Select `porpoise-api` service
3. Go to **Variables** tab
4. Add or verify:
   ```
   RAILWAY_MAX_REQUEST_SIZE=100MB
   ```
5. Deploy/restart the service

## Why This Matters

- **Railway's default limit:** 10MB
- **Without this variable:** Uploads over 10MB get `413 Payload Too Large` error
- **With this variable:** Uploads up to 100MB are allowed

The application code is already configured for 100MB uploads, but Railway's proxy will block requests before they reach the app unless this environment variable is set.

## Verification

After setting the variable and deploying:

1. Try uploading a file between 10-100MB
2. Should succeed without 413 errors
3. Check Railway logs if issues persist

## Reference

See [docs/RAILWAY_DEPLOYMENT.md](docs/RAILWAY_DEPLOYMENT.md) for complete deployment documentation.
