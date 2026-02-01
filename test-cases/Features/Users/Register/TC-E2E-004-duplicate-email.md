# Test Case: [TC-E2E-004] Duplicate Email - Negative
- **Page**: /register | **Category**: negative | **Priority**: P1

## Preconditions
- User "alice@example.com" already exists in database

## Steps
1. Login via Auth0 with alice@example.com claims
2. Attempt to submit registration with same email

## Expected Results
- API returns 409 Conflict or validation error
- Snackbar shows appropriate error message
- User remains on /register page
- No duplicate record created

## Edge Cases
- Case sensitivity: "Alice@Example.com" vs "alice@example.com"
- Trimmed whitespace: " alice@example.com "

## Status: [ ] Not Tested
