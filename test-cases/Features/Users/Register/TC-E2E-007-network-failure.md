# Test Case: [TC-E2E-007] Network Failure During Submit

- **Page**: /register | **Category**: negative | **Priority**: P1

## Preconditions

- Valid form data entered
- Button enabled

## Steps

1. Simulate network failure (disable network/API)
2. Click "COMPLETE REGISTRATION"
3. Wait for timeout

## Expected Results

- Button changes to "PROCESSING"
- After timeout, error snackbar appears
- Button returns to enabled state
- User can retry submission
- No partial data saved

## Status

[ ] Not Tested
