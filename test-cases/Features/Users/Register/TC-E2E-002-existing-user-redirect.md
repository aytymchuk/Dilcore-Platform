# Test Case: [TC-E2E-002] Existing User - Automatic Redirect

- **Page**: /register
- **Category**: e2e-functional
- **Priority**: P0

## Preconditions

- User already registered in the system
- User authenticated via Auth0

## Steps

1. Navigate directly to /register
2. Wait for page initialization

## Expected Results

- User is automatically redirected to / (home)
- No registration form shown
- No error snackbar (silent redirect)

## Technical Flow

1. `OnInitializedAsync` calls `IIdentityClient.SafeGetCurrentUserAsync()`
2. If user exists → `AppNavigator.ToHome(forceLoad: true)`
3. If user not found → show registration form

## Status

[ ] Not Tested
