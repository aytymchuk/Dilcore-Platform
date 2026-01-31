# Test Case: [TC-E2E-003] Multiple User Accounts
- **Page**: /register | **Category**: e2e-functional | **Priority**: P1

## Preconditions
- Multiple Auth0 accounts available
- WebApp supports multi-tenant or multi-user

## Scenario A: Register User 1
1. Login with user1@example.com
2. Complete registration: "Alice", "Smith"
3. Verify home page shows "Alice Smith"
4. Logout

## Scenario B: Register User 2
1. Login with user2@example.com
2. Complete registration: "Bob", "Johnson"
3. Verify home page shows "Bob Johnson"
4. Logout

## Scenario C: Switch Between Users
1. Login as user1@example.com
2. Verify sees own profile (Alice Smith)
3. Logout
4. Login as user2@example.com
5. Verify sees own profile (Bob Johnson)

## Expected Results
- Each user has independent profile
- No data leakage between sessions
- Correct user data displayed for each login

## Status: [ ] Not Tested | **BLOCKED**: Auth0 logout (#79)
