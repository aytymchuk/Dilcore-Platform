# Test Case: [TC-E2E-001] New User Registration - Complete Flow
- **Page**: /register | **Category**: e2e-functional | **Priority**: P0

## Preconditions
- Fresh Auth0 account (not registered in system)
- WebApp and WebApi running

## Steps
1. Login via Auth0 with new account
2. Verify redirect to /register
3. Verify email pre-filled from Auth0 claims
4. Enter First Name: "John"
5. Enter Last Name: "Doe"
6. Click "COMPLETE REGISTRATION"
7. Wait for processing

## Expected Results
- Button shows "PROCESSING" with spinner
- API call: POST /api/identity/users/register
- Snackbar: "Registration successful! Welcome to the platform."
- Redirect to / (homepage)
- User profile persisted in database

## Verification
- Homepage shows user name
- /register redirects to homepage (already registered)

## Status: [ ] Not Tested - **BLOCKED**: Requires fresh Auth0 account
