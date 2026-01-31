# Test Case: [TC-E2E-005] Session Expiration During Registration

- **Page**: /register | **Category**: edge-cases | **Priority**: P2

## Preconditions

- User on /register page with form filled
- Auth0 token about to expire

## Steps

1. Fill registration form completely
2. Wait for Auth0 token to expire (or simulate)
3. Click "COMPLETE REGISTRATION"

## Expected Results

- API returns 401 Unauthorized
- User redirected to Auth0 login
- After re-login, user returns to /register
- Form data may be lost (acceptable)

## Technical Considerations

- Token refresh should be attempted first
- If refresh fails, redirect to login
- Consider preserving form state in localStorage

## Status

[ ] Not Tested
