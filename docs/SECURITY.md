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

### Automated Integration Tests (Future)

For production systems with real customer data, add automated security tests:

**Create:** `Porpoise.Api.Tests/Integration/SecurityTests.cs`

```csharp
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Porpoise.Api.Tests.Integration;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RateLimiting_BlocksAfter100Requests()
    {
        // Arrange: Make 100 successful requests
        for (int i = 0; i < 100; i++)
        {
            var response = await _client.GetAsync("/health");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Act: Make 101st request
        var blockedResponse = await _client.GetAsync("/health");

        // Assert: Should be rate limited
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedResponse.StatusCode);
        var content = await blockedResponse.Content.ReadAsStringAsync();
        Assert.Contains("Too many requests", content);
    }

    [Fact]
    public async Task CORS_AllowsWhitelistedOrigins()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://porpoiseanalytics.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal("https://porpoiseanalytics.com", allowedOrigin);
    }

    [Fact]
    public async Task CORS_BlocksUnknownOrigins()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", "https://evil-hacker-site.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert: Should not have CORS headers for disallowed origin
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("https://porpoiseanalytics.com")]
    [InlineData("https://www.porpoiseanalytics.com")]
    [InlineData("https://pulse-ui-production.up.railway.app")]
    [InlineData("https://pulse-ui-staging.up.railway.app")]
    [InlineData("http://localhost:5173")]
    public async Task CORS_AllowsAllWhitelistedOrigins(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/health");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal(origin, allowedOrigin);
    }

    [Fact]
    public async Task ForwardedHeaders_ExtractsClientIPForRateLimiting()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Forwarded-For", "192.168.1.100");

        // Act: Make 101 requests with same forwarded IP
        for (int i = 0; i < 100; i++)
        {
            var response = await _client.SendAsync(request.Clone());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        var blockedResponse = await _client.SendAsync(request);

        // Assert: Should be rate limited based on X-Forwarded-For
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedResponse.StatusCode);
    }
}

// Helper extension for cloning requests
public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage Clone(this HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        foreach (var header in request.Headers)
        {
            clone.Headers.Add(header.Key, header.Value);
        }
        return clone;
    }
}
```

**When to implement:**
- ✅ Now: Manual testing (documented above) is sufficient for demo data
- ⏳ Later: Add automated tests before deploying with real customer data
- ⏳ Future: Run tests in CI/CD pipeline to catch security regressions

**Benefits of automated tests:**
- Catch security configuration changes in CI/CD
- Verify rate limiting works after dependency updates
- Ensure CORS whitelist remains correct
- Test forwarded header handling for proxy environments
- Document expected security behavior in code

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

## Payment Processing & PCI Compliance (Future)

### DO NOT Handle Credit Cards Directly

**Never store credit card numbers, CVVs, or expiration dates in your database.**

Use a payment processor that handles PCI compliance for you:
- **Stripe** (Recommended) - Excellent .NET SDK, great docs, 2.9% + 30¢ per transaction
- **Paddle** - Merchant of record, handles sales tax/VAT
- **Braintree** (PayPal) - Enterprise-focused

### How Payment Processing Works

**User Flow:**
1. User enters card on **Stripe's hosted page** or embedded Stripe Elements widget
2. **Stripe tokenizes** the card - you never see the actual card number
3. You receive a **token/customer ID** - store this in your database
4. **Charge via Stripe API** - pass the token, Stripe processes payment
5. **Stripe handles:** PCI compliance, fraud detection, chargebacks, refunds

**What Stripe Handles (You Don't Worry About):**
- ✅ PCI DSS compliance (Level 1 certified)
- ✅ Card number encryption and storage
- ✅ Fraud detection and prevention
- ✅ International payments and currency conversion
- ✅ 3D Secure authentication (SCA compliance)
- ✅ Dispute/chargeback management
- ✅ Payment method diversification (cards, ACH, wallets)

**What You're Responsible For:**
- ✅ User authentication - Ensure right user is logged in
- ✅ Authorization - Users can only manage their own subscriptions
- ✅ Webhook security - Verify Stripe webhook signatures
- ✅ Subscription logic - Track plan levels, features, limits
- ✅ Basic PII protection - Email addresses, names (not card data)
- ✅ Audit logging - Who subscribed/cancelled when

### Data Model

**Store only Stripe references, never card data:**

```csharp
public class Subscription
{
    public int Id { get; set; }
    public int TenantId { get; set; }  // Links to your customer/organization
    
    // Stripe references (safe to store)
    public string StripeCustomerId { get; set; }  // "cus_abc123"
    public string StripeSubscriptionId { get; set; }  // "sub_xyz789"
    public string? StripePaymentMethodId { get; set; }  // "pm_card_xyz"
    
    // Your business logic
    public string PlanLevel { get; set; }  // "basic", "pro", "enterprise"
    public SubscriptionStatus Status { get; set; }  // Active, Cancelled, PastDue
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Cancelled,
    Unpaid
}
```

**NEVER store:**
- ❌ Credit card numbers
- ❌ CVV/CVC codes
- ❌ Expiration dates
- ❌ Full card PANs (Primary Account Numbers)

### Implementation Steps

1. **Create Stripe Account**
   - Sign up at stripe.com
   - Get API keys (test and live)
   - Store in environment variables (never commit keys)

2. **Install Stripe .NET SDK**
   ```bash
   dotnet add package Stripe.net
   ```

3. **Create Stripe Customer on User Signup**
   ```csharp
   var customerService = new CustomerService();
   var customer = await customerService.CreateAsync(new CustomerCreateOptions
   {
       Email = user.Email,
       Name = user.Name,
       Metadata = new Dictionary<string, string>
       {
           { "tenant_id", tenant.Id.ToString() }
       }
   });
   
   // Store customer.Id in your database
   tenant.StripeCustomerId = customer.Id;
   ```

4. **Create Checkout Session** (user subscribes)
   ```csharp
   var sessionService = new SessionService();
   var session = await sessionService.CreateAsync(new SessionCreateOptions
   {
       Customer = tenant.StripeCustomerId,
       Mode = "subscription",
       LineItems = new List<SessionLineItemOptions>
       {
           new SessionLineItemOptions
           {
               Price = "price_pro_plan_monthly",  // From Stripe dashboard
               Quantity = 1
           }
       },
       SuccessUrl = "https://porpoiseanalytics.com/subscribe/success",
       CancelUrl = "https://porpoiseanalytics.com/subscribe/cancel"
   });
   
   // Redirect user to session.Url (Stripe hosted page)
   ```

5. **Handle Webhooks** (subscription events)
   ```csharp
   [ApiController]
   [Route("api/webhooks/stripe")]
   public class StripeWebhookController : ControllerBase
   {
       [HttpPost]
       public async Task<IActionResult> HandleWebhook()
       {
           var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
           var stripeSignature = Request.Headers["Stripe-Signature"];
           
           try
           {
               var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
               var stripeEvent = EventUtility.ConstructEvent(
                   json, stripeSignature, webhookSecret
               );
               
               switch (stripeEvent.Type)
               {
                   case "customer.subscription.created":
                       // Update subscription status to Active
                       break;
                   case "customer.subscription.updated":
                       // Handle plan changes, renewals
                       break;
                   case "customer.subscription.deleted":
                       // Mark subscription as cancelled
                       break;
                   case "invoice.payment_failed":
                       // Notify user, mark as past_due
                       break;
               }
               
               return Ok();
           }
           catch (StripeException)
           {
               return BadRequest();
           }
       }
   }
   ```

6. **Check Subscription Before API Access**
   ```csharp
   public class SubscriptionMiddleware
   {
       public async Task InvokeAsync(HttpContext context)
       {
           var tenantId = context.User.FindFirst("tenant_id")?.Value;
           var subscription = await _db.Subscriptions
               .FirstOrDefaultAsync(s => s.TenantId == int.Parse(tenantId));
           
           if (subscription?.Status != SubscriptionStatus.Active)
           {
               context.Response.StatusCode = 402;  // Payment Required
               await context.Response.WriteAsync("Subscription required");
               return;
           }
           
           await _next(context);
       }
   }
   ```

### Security Considerations

**Webhook Security:**
- Always verify Stripe signatures on webhooks
- Use raw request body for signature verification
- Reject requests with invalid signatures

**API Keys:**
- Store in environment variables, never in code
- Use test keys in development
- Use restricted API keys (limit permissions)
- Rotate keys periodically

**Audit Logging:**
- Log all subscription changes (created, updated, cancelled)
- Track payment failures and retries
- Monitor for suspicious activity (rapid subscribe/cancel)

**User Privacy:**
- Only store necessary PII (email, name)
- Encrypt PII at rest if handling EU customers (GDPR)
- Provide export/delete functionality for user data

### Compliance Requirements

**PCI DSS:** Handled by Stripe (you're not in scope if you never touch card data)

**GDPR (if EU customers):**
- ✅ Get consent before processing payments
- ✅ Allow users to export their data
- ✅ Allow users to delete their account and data
- ✅ Have a privacy policy
- ✅ Appoint a DPO if processing large scale

**SCA (Strong Customer Authentication - EU):**
- ✅ Handled by Stripe automatically
- ✅ Uses 3D Secure for EU cards

### Testing

**Use Stripe Test Mode:**
- Test card: `4242 4242 4242 4242`
- Any future expiration, any CVV
- Test webhooks locally with Stripe CLI

**Stripe CLI:**
```bash
# Install Stripe CLI
brew install stripe/stripe-cli/stripe

# Forward webhooks to localhost
stripe listen --forward-to localhost:5107/api/webhooks/stripe

# Trigger test webhook
stripe trigger customer.subscription.created
```

### Pricing Strategy Examples

**SaaS Tiers:**
- Free: 1 project, 100 responses/month, no AI
- Pro ($29/mo): Unlimited projects, 10k responses/month, AI insights
- Enterprise ($99/mo): Unlimited everything, priority support, white-label

**Usage-Based:**
- Base: $10/mo + $0.01 per response processed
- Good for variable usage patterns

### Checklist for Payment Processing

Before accepting real payments:

- [ ] Stripe account created and verified
- [ ] Test mode working end-to-end
- [ ] Webhook endpoint secured with signature verification
- [ ] Subscription status checked before API access
- [ ] User can view/manage their subscription
- [ ] Cancellation flow implemented
- [ ] Failed payment notifications sent to users
- [ ] Privacy policy includes payment processing (mention Stripe)
- [ ] Terms of service include refund policy
- [ ] Audit logging for all subscription events
- [ ] Test all webhooks (created, updated, deleted, payment_failed)
- [ ] Handle edge cases (subscription past_due, cancelled with access until period end)

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Railway Security Best Practices](https://docs.railway.app/guides/security)
- [Auth0 Documentation](https://auth0.com/docs)
- [Stripe Documentation](https://stripe.com/docs)
- [Stripe .NET SDK](https://github.com/stripe/stripe-dotnet)
- [PCI DSS Compliance](https://www.pcisecuritystandards.org/)
- [GDPR Overview](https://gdpr.eu/)

## Contact

For security concerns or to report vulnerabilities, contact the development team immediately.

**Last Updated:** December 24, 2025
