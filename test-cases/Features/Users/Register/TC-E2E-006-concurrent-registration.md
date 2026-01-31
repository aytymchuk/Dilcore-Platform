# Test Case: [TC-E2E-006] Concurrent Registration Attempts

- **Page**: /register | **Category**: edge-cases | **Priority**: P2

## Preconditions

- Same Auth0 user logged in on two browser tabs
- User not yet registered in system

## Steps

1. Open /register in Tab A
2. Open /register in Tab B
3. Fill form in Tab A
4. Fill form in Tab B
5. Click submit in Tab A
6. Quickly click submit in Tab B

## Expected Results

- Tab A: Registration succeeds
- Tab B: Receives error "User already exists" or similar
- No duplicate user records in database
- At least one registration completes successfully

## Technical Considerations

- Database unique constraint on email/user ID
- Idempotent registration endpoint
- Race condition handling

## Status

[ ] Not Tested
