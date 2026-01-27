---
trigger: glob
globs: *.cs
---

# Coding Guidelines

## Overview
This document outlines the mandatory coding standards and best practices for the Dilcore Platform. These rules ensure consistency, maintainability, and readability across the codebase.

## 1. Constants & Magic Values
- **No Magic Values**: Do not use hardcoded strings or numbers inline.
- **Location**: Place all magic values in appropriate `Constants` classes.
- **Structure**: Constant classes must be `static`.
  ```csharp
  public static class UserConstants
  {
      public const int MaxNameLength = 100;
      public const string DefaultLanguage = "en-US";
  }
  ```

## 2. Control Structures
- **Braces Required**: `if` and `else` statements MUST always use braces `{ }`, even for single-line blocks.
  ```csharp
  // correct
  if (isValid)
  {
      DoSomething();
  }

  // incorrect
  if (isValid) DoSomething();
  ```

## 3. Asynchronous Methods
- **Naming**: Methods returning `Task` or `Task<T>` MUST end with the suffix `Async`.
- **Cancellation**: Accept a `CancellationToken` parameter if any nested async calls support it.
  ```csharp
  public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
  {
      return await _repository.GetAsync(id, cancellationToken);
  }
  ```

## 4. Class Design
- **Sealed Classes**: Do NOT make classes `sealed` by default.
- **Refactoring**: If you encounter a `sealed` class that does not have a strict security or functional requirement to be sealed, remove the `sealed` keyword.

## 5. Formatting & Whitespace
- **Logical Separation**: Separate logical parts of code with exactly **one** empty line.
- **Block Padding**: Do NOT leave empty lines at the very beginning or end of a code block (method body, class body).
  ```csharp
  public void ProcessData()
  {
      // No empty line here
      var data = GetData();

      Validate(data); // One empty line before this block

      Save(data);
      // No empty line here
  }
  ```

## 6. Naming Conventions implementation
- **Private Fields**: Must start with an underscore and be `_camelCase`.
  ```csharp
  private readonly ILogger _logger;
  private string _userName;
  ```
- **Properties**: Must be `PascalCase`.
  ```csharp
  public string FirstName { get; set; }
  ```

## 7. Member Ordering
Members in a class must be ordered strictly as follows:
1. **Constants**
2. **Private Static Fields**
3. **Private Read-Only Fields**
4. **Internal Properties**
5. **Public Properties**
6. **Public Methods**
7. **Internal Methods**
8. **Private Methods**
9. **Static Members** (Public then Private)

## 8. Single Responsibility Principle
- **Method Scope**: A method should perform **strictly one action**.
- **Refactoring**: If a method is complex or combines multiple distinct logic flows, split it into smaller, focused private methods. The caller method should orchestrate these smaller methods.

## 9. Codebase Specific Standards
- **Use Records**: Prefer `record` types for Domain Entities and DTOs where appropriate (immutability).
- **Feature Folders**: Organize UI logic in feature-specific folders (e.g., `Features/Users/Register`).
- **MediatR**: Use Command/Query pattern (MediatR) for business logic.
- **Validation**: Use FluentValidation for all model validation.
