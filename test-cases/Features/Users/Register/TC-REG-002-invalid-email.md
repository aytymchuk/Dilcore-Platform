# Test Case: [TC-REG-002] Invalid Email Format Validation
- **Page**: /register | **Category**: negative | **Priority**: P1

## Preconditions
- User authenticated via Auth0
- Register page loaded

## Steps
1. Clear email field
2. Enter invalid email (e.g., "notanemail", "invalid-format")
3. Tab out of field to trigger blur validation

## Expected Results
- Red error text: "Please enter a valid email address"
- Submit button remains disabled

## Status: [x] Passed
