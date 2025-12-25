# Security Configuration

## Overview

This document outlines the security measures implemented in the Porpoise application and recommendations for future enhancements.

## Current Security Measures

### HTTPS/TLS
✅ **Status: Enabled**
- Railway automatically provides HTTPS for all deployed services
- Production: `https://pulse-ui-production.up.railway.app` and `https://porpoiseanalytics.com`
- Staging: `https://pulse-ui-staging.up.railway.app`
- All traffic is encrypted in transit

### CORS (Cross-Origin Resource Sharing)
✅ **Status: Configured**
- **Location:** `Porpoise.Api/Program.cs`
- **Configuration:** Restricted to specific origins
  - `https://porpoiseanalytics.com`
  - `https://www.porpoiseanalytics.com`
  - `https://pulse-ui-production.up.railway.app`
  - `https://pulse-ui-staging.up.railway.app`
  - `http://localhost:5173` (local development only)
- **Purpose:** Prevents unauthorized websites from making requests to the API

### Rate Limiting
✅ **Status: Enabled**
- **Location:** `Porpoise.Api/Program.cs`
- **Implementation:** .NET built-in rate limiting middleware
- **Limits:** 100 requests per minute per IP address
- **Algorithm:** Fixed window limiter
- **Purpose:** Prevents API abuse, DoS attacks, and accidental infinite loops
- **Response:** HTTP 429 (Too Many Requests) when limit exceeded

## Current State: Demo Data Only

The application is currently deployed with **demo data only** and no authentication. This is acceptable for the following reasons:

- No sensitive or personal information
- Public showcase/portfolio site
- No user accounts or saved preferences
- All data is read-only demonstration content

## Security Gaps (Acceptable for Demo)

### No Authentication/Authorization
❌ **Status: Not Implemented**
- All API endpoints are publicly accessible
- No user login or session management
- No API keys or tokens required

### No Input Validation
⚠️ **Status: Basic validation only**
- Framework-level validation exists
- No comprehensive input sanitization
- No SQL injection protection needed (using EF Core with parameterized queries)

### No Audit Logging
❌ **Status: Not Implemented**
- No tracking of API requests
- No security event logging
- No monitoring of suspicious activity

## Backend Accessibility

**Important:** The API backend **IS publicly accessible** at:
- Production: `https://porpoise-api-production.up.railway.app`
- Staging: `https://porpoise-api-staging.up.railway.app`

Anyone can call API endpoints directly (not just through the UI). Rate limiting provides basic protection against abuse.

## Before Moving to Production with Real Data

When the application will handle real customer data, the following security measures **must** be implemented:

### 1. Authentication & Authorization
**Priority: CRITICAL**

Options to consider:
- **Auth0:** Third-party auth platform (fastest to implement)
- **Azure AD B2C:** Microsoft identity platform
- **Custom JWT:** Roll your own token-based auth

Implementation needs:
- User registration and login
- JWT token generation and validation
- Secure password hashing (BCrypt, Argon2)
- Password reset flow
- Session management

### 2. Multi-Tenant Security
**Priority: CRITICAL**

The application already has `TenantMiddleware` scaffolding but needs:
- Tenant identification from authenticated user
- Database-level tenant isolation
- Row-level security ensuring users only see their own data
- Tenant context validation on every request

### 3. API Security Enhancements
**Priority: HIGH**

- API key authentication for external integrations
- Stricter rate limiting (per user, not just per IP)
- Request size limits
- Input validation and sanitization
- Output encoding to prevent XSS

### 4. Database Security
**Priority: HIGH**

- SSL/TLS for database connections (Railway supports this)
- Principle of least privilege for database users
- Separate read-only users for reporting
- Database audit logging
- Regular backups (verify restore process)
- Encryption at rest (verify Railway provides this)

### 5. Monitoring & Logging
**Priority: MEDIUM**

- Centralized logging (Serilog, Application Insights)
- Security event monitoring
- Failed login attempt tracking
- API abuse detection
- Real-time alerting for suspicious activity

### 6. Additional Best Practices
**Priority: MEDIUM**

- OWASP Top 10 compliance review
- Regular dependency updates and vulnerability scanning
- Security headers (CSP, HSTS, X-Frame-Options)
- Penetration testing
- Security incident response plan
- Data retention and deletion policies
- GDPR/privacy compliance if handling EU users

## Configuration by Environment

### Development (Local)
- CORS allows `localhost:5173`
- Rate limiting: 100 req/min (same as production)
- No authentication required
- Use in-memory or local MySQL database

### Staging
- CORS restricted to staging URL
- Rate limiting enabled
- Matches production security configuration
- Uses separate MySQL database with copy of production data

### Production
- CORS restricted to production domains only
- Rate limiting enabled
- All security measures enforced
- Separate MySQL database

## Testing Security Configuration

### CORS Testing
```bash
# Should succeed (allowed origin)
curl -H "Origin: https://porpoiseanalytics.com" \
     -H "Access-Control-Request-Method: GET" \
     -X OPTIONS \
     https://porpoise-api-production.up.railway.app/api/projects

# Should fail (disallowed origin)
curl -H "Origin: https://malicious-site.com" \
     -H "Access-Control-Request-Method: GET" \
     -X OPTIONS \
     https://porpoise-api-production.up.railway.app/api/projects
```

### Rate Limiting Testing
```bash
# Send 101 requests in quick succession (should get 429 on 101st)
for i in {1..101}; do
  curl https://porpoise-api-production.up.railway.app/health
done
```

## Security Checklist for Production

Before deploying with real user data:

- [ ] Implement user authentication (Auth0/Azure AD/Custom JWT)
- [ ] Add authorization middleware
- [ ] Enable tenant isolation in TenantMiddleware
- [ ] Add comprehensive input validation
- [ ] Implement audit logging
- [ ] Set up monitoring and alerting
- [ ] Configure database SSL/TLS
- [ ] Review and test all security headers
- [ ] Perform security audit/penetration test
- [ ] Document incident response procedures
- [ ] Set up automated dependency scanning
- [ ] Review privacy policy and terms of service
- [ ] Implement data backup and recovery process
- [ ] Test disaster recovery procedures

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Railway Security Best Practices](https://docs.railway.app/guides/security)
- [Auth0 Documentation](https://auth0.com/docs)

## Contact

For security concerns or to report vulnerabilities, contact the development team immediately.

**Last Updated:** December 24, 2025
