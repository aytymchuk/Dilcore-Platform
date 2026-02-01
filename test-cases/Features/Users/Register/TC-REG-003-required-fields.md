# Test Case: [TC-REG-003] Required Field Validation
- **Page**: /register | **Category**: negative | **Priority**: P1

## Preconditions
- Register page loaded

## Steps
1. Leave First Name empty
2. Leave Last Name empty
3. Tab through fields

## Expected Results
- Error: "First name is required"
- Error: "Last name is required"
- Submit button disabled

## Status: [x] Passed
