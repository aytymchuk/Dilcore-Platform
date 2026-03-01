---
description: Build the solution and run all tests, fixing any errors iteratively.
---

## Step 1 — Build

Run the full solution build:

```bash
dotnet build Dilcore.Platform.sln
```

If the build fails:

1. Collect **all** build errors from the output.
2. For each error, investigate the root cause in the referenced file and line — do not blindly apply fixes.
3. Apply fixes for all errors.
4. Re-run the build.
5. Repeat until the build succeeds with zero errors.

## Step 2 — Tests

Once the build is green, run all tests:

```bash
dotnet test Dilcore.Platform.sln --no-build --verbosity normal
```

If any tests fail:

1. Collect every failed test name and its error message.
2. For each failure, **read the test code and the assertions first**, then read the production code under test.
3. Determine whether the defect is in the production logic or in the test itself:
   - If production logic is wrong or broken — fix the production code.
   - If the test has incorrect expectations or setup — fix the test.
4. After applying all fixes, re-run `dotnet test Dilcore.Platform.sln --no-build --verbosity normal`.
5. Repeat until all tests pass.
