# Test Case: [TC-E2E-010] Unauthenticated Access

- **Page**: /register | **Category**: negative | **Priority**: P0

## Preconditions

- User NOT logged in (no Auth0 session)
- Incognito browser window

## Steps

1. Navigate directly to /register (or http://localhost:5200/register if testing locally)
2. Observe behavior

## Expected Results

- Redirect to Auth0 login page
- After Auth0 login, return to /register
- Registration form shown if new user
- Or redirect to home if existing user

## Security Check

- No registration form visible without authentication
- Cannot submit registration without valid Auth0 token

## Status

[ ] Not Tested
