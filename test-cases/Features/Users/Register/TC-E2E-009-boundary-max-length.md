# Test Case: [TC-E2E-009] Boundary Value - Max Length Names

- **Page**: /register | **Category**: edge-cases | **Priority**: P2

## Preconditions

- Register page loaded

## Test Data

| Field | Length | Expected |
|-------|--------|----------|
| First Name | 99 chars | ✅ Valid |
| First Name | 100 chars | ✅ Valid (boundary) |
| First Name | 101 chars | ❌ Error |
| Last Name | 99 chars | ✅ Valid |
| Last Name | 100 chars | ✅ Valid (boundary) |
| Last Name | 101 chars | ❌ Error |

## Steps

1. Generate 100-character string
2. Enter in First Name
3. Submit form
4. Verify no error shown
5. Repeat with 101 characters
6. Verify validation error shown

## Expected Results

- First Name and Last Name: ≤100 chars accepted; saved correctly
- First Name and Last Name: >100 chars show validation error (e.g. "... cannot exceed 100 characters")

## Status

[ ] Not Tested
