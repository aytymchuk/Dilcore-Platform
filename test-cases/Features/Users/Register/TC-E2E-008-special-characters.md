# Test Case: [TC-E2E-008] Special Characters in Names

- **Page**: /register | **Category**: edge-cases | **Priority**: P2

## Preconditions

- Register page loaded
- Valid email pre-filled

## Test Data

| First Name | Last Name | Expected |
|------------|-----------|----------|
| O'Brien | McDonald | ✅ Valid |
| José | García | ✅ Valid (Unicode) |
| 李 | 明 | ✅ Valid (Chinese) |
| Anne-Marie | Smith-Jones | ✅ Valid (hyphen) |
| `<script>` | `alert()` | ❌ Escaped/Rejected |
| -- | DROP TABLE | ❌ Escaped/Safe |

## Expected Results

- Valid names with apostrophes, hyphens, unicode accepted
- HTML/SQL injection attempts safely handled
- Names stored and displayed correctly
- No XSS vulnerabilities

## Status

[ ] Not Tested
