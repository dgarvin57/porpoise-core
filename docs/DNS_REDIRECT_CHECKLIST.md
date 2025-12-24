# Domain, DNS, and Redirect Checklist

Primary: porpoiseanalytics.com
Optional/defensive: purposeanalytics.com, porpoiseanalytics.ai, porpoise-analytics.com (defensive only)

## Core Setup
- Registrar: purchase domains (primary + defensive)
- DNS: choose provider (Cloudflare or Namecheap DNS). Set nameservers.
- A/AAAA: point `porpoiseanalytics.com` and `www` to hosting (IP or CNAME).
- TLS: provision certificates for all domains; auto‑renew enabled.

## Redirects & Canonicals
- 301: redirect `www` → apex (`www.porpoiseanalytics.com` → `porpoiseanalytics.com`).
- 301: redirect alternates (`purposeanalytics.com`, `porpoise-analytics.com`, `porpoiseanalytics.ai`) → `porpoiseanalytics.com`.
- Canonical: set `<link rel="canonical">` to the destination URL on all pages.

## Email Deliverability (primary on .com)
- MX: configure mail provider for `porpoiseanalytics.com`.
- SPF: add TXT record (allow sending services, e.g., `v=spf1 include:mailprovider ~all`).
- DKIM: publish selector TXT records from your mail service.
- DMARC: add TXT `_dmarc` (start relaxed: `v=DMARC1; p=none; rua=mailto:dmarc@porpoiseanalytics.com`).

## Subdomains (optional)
- `app.porpoiseanalytics.com`: product app.
- `api.porpoiseanalytics.com`: backend API.
- `status.porpoiseanalytics.com`: status page.

## Testing
- Curl 301 chains: confirm single hop to canonical.
- Check SSL for all domains (SSLLabs).
- Verify SPF/DKIM/DMARC with mail-tester.
- Google Search Console: submit sitemap; verify canonical.

## Notes
- Hyphenated domains are for defensive capture only; avoid using them in marketing (radio test + typo risk).
- `.ai` is vanity/defensive; redirect to `.com`.
