# Go-Live Checklist — Pilot Customer

Purpose: hand a working instance to the first (pilot) customer and catch
problems before they do. Complements `TESTING_SCENARIOS.md` (detailed manual
UI scenarios) and `docs/DEPLOYMENT.md` (server setup).

## 1. Before deploying

- [ ] All CI checks green on `main`
- [ ] Manual UI pass of `TESTING_SCENARIOS.md` sections 1–3 in a browser
      (automated tests cover the API; the click-through covers the UI wiring)
- [ ] `.env.production` filled: real domain in `PUBLIC_URL`, fresh
      `JWT_SECRET` + `POSTGRES_PASSWORD`, working SMTP account,
      `ADMIN_NOTIFICATION_EMAIL` set to an inbox someone actually reads
- [ ] Send a test email through the SMTP account from the server
      (many providers block new servers — verify before launch, not after)
- [ ] HTTPS works on the domain (auth cookies are `Secure` — login silently
      fails over plain HTTP)

## 2. First hour after deploying

- [ ] `docker-compose ps` shows every container healthy (healthcheck hits `/health`)
- [ ] Log in as `admin@storefront.com` and **change the seeded password**
- [ ] Set Site name/contact email + feature flags in admin Settings
- [ ] Create the customer's real partner account from the admin panel
- [ ] Full round-trip as that partner **from a phone**: login → browse →
      cart → order → verify the admin notification email arrived
- [ ] Update the order status as admin → verify the partner status email arrived
- [ ] Trigger "forgot password" for the partner → reset link email arrives
      and works on the real domain
- [ ] `scripts/backup-db.sh` runs cleanly; cron entry installed; test that
      the dump restores (`scripts/restore-db.sh`) on a scratch database

## 3. During the pilot

- [ ] Check `docker-compose logs api` (or `logs/storefront-*.log`) for `ERR`
      lines every few days — Serilog captures what users won't report
- [ ] Agree with the pilot user on ONE channel for bug reports (e.g. a
      WhatsApp group) and ask for screenshots + the time it happened —
      timestamps let you find the exact log lines
- [ ] Watch the first real order end-to-end: request → status updates →
      delivery — this exercises every notification path with real data
- [ ] Before any upgrade during the pilot: run `backup-db.sh` first, then
      `git pull` + `up -d --build` (migrations apply automatically)

## Known limitations to communicate to the pilot

- Admin users have no self-service password reset (partner users do);
  admin resets are done in the Users panel
- Pricing is off by default (`Features.Pricing.Enabled`) — the system runs
  as a quote-request flow unless the flag is enabled in Settings
- The mobile app is partner-only and needs an EAS build pointed at the
  production API URL (`mobile/eas.json`)
