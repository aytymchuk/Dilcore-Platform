using FluentResults;
using Shouldly;

namespace Dilcore.Tests.Common;

/// <summary>
/// Shouldly extension methods for FluentResults Result types.
/// </summary>
public static class FluentResultsShouldlyExtensions
{
    #region Success Assertions

    /// <summary>
    /// Asserts that the result is successful.
    /// </summary>
    public static void ShouldBeSuccess<T>(this Result<T> result, string? customMessage = null)
    {
        result.IsSuccess.ShouldBeTrue(customMessage ?? $"Expected result to be successful, but it failed with errors: {FormatErrors(result)}");
    }

    /// <summary>
    /// Asserts that the result is successful.
    /// </summary>
    public static void ShouldBeSuccess(this Result result, string? customMessage = null)
    {
        result.IsSuccess.ShouldBeTrue(customMessage ?? $"Expected result to be successful, but it failed with errors: {FormatErrors(result)}");
    }

    /// <summary>
    /// Asserts that the result is successful and the value is not null.
    /// </summary>
    public static T ShouldBeSuccessWithValue<T>(this Result<T> result, string? customMessage = null)
    {
        result.ShouldBeSuccess(customMessage);
        ((object?)result.Value).ShouldNotBeNull(customMessage ?? "Expected result value to not be null");
        return result.Value;
    }

    /// <summary>
    /// Asserts that the result is successful and the value is null.
    /// </summary>
    public static void ShouldBeSuccessWithNullValue<T>(this Result<T?> result, string? customMessage = null)
        where T : class
    {
        result.ShouldBeSuccess(customMessage);
        result.Value.ShouldBeNull(customMessage ?? "Expected result value to be null");
    }

    #endregion

    #region Failure Assertions

    /// <summary>
    /// Asserts that the result is failed.
    /// </summary>
    public static void ShouldBeFailed<T>(this Result<T> result, string? customMessage = null)
    {
        result.IsFailed.ShouldBeTrue(customMessage ?? "Expected result to be failed, but it was successful");
    }

    /// <summary>
    /// Asserts that the result is failed.
    /// </summary>
    public static void ShouldBeFailed(this Result result, string? customMessage = null)
    {
        result.IsFailed.ShouldBeTrue(customMessage ?? "Expected result to be failed, but it was successful");
    }

    /// <summary>
    /// Asserts that the result is failed (works with any result type).
    /// </summary>
    public static void ShouldBeFailed(this IResultBase result, string? customMessage = null)
    {
        result.IsFailed.ShouldBeTrue(customMessage ?? "Expected result to be failed, but it was successful");
    }

    /// <summary>
    /// Asserts that the result is failed and contains an error of the specified type.
    /// Works with any result type (Result, Result&lt;T&gt;) via IResultBase.
    /// </summary>
    public static TError ShouldBeFailedWithError<TError>(this IResultBase result, string? customMessage = null)
        where TError : class, IError
    {
        result.ShouldBeFailed(customMessage);
        var error = result.Errors.OfType<TError>().FirstOrDefault();
        error.ShouldNotBeNull(customMessage ?? $"Expected result to contain error of type {typeof(TError).Name}, but found: {FormatErrors(result)}");
        return error;
    }

    /// <summary>
    /// Asserts that the result is failed and contains an error with the specified message.
    /// Works with any result type via IResultBase.
    /// </summary>
    public static void ShouldBeFailedWithMessage(this IResultBase result, string expectedMessage, string? customMessage = null)
    {
        result.ShouldBeFailed(customMessage);
        result.Errors.ShouldContain(
            e => e.Message.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase),
            customMessage ?? $"Expected result to contain error with message '{expectedMessage}', but found: {FormatErrors(result)}");
    }

    /// <summary>
    /// Asserts that the result is failed and allows custom assertions on the errors collection.
    /// Works with any result type via IResultBase.
    /// </summary>
    /// <example>
    /// result.ShouldBeFailedWithErrors(errors =>
    /// {
    ///     errors.Count.ShouldBe(2);
    ///     errors.ShouldContain(e => e is ValidationError);
    /// });
    /// </example>
    public static void ShouldBeFailedWithErrors(this IResultBase result, Action<IReadOnlyList<IError>> errorAssertions, string? customMessage = null)
    {
        result.ShouldBeFailed(customMessage);
        errorAssertions(result.Errors);
    }

    /// <summary>
    /// Asserts that the result is failed and contains an error of the specified type with the specified message.
    /// Works with any result type via IResultBase.
    /// </summary>
    public static TError ShouldBeFailedWithErrorAndMessage<TError>(this IResultBase result, string expectedMessage, string? customMessage = null)
        where TError : class, IError
    {
        result.ShouldBeFailed(customMessage);
        var error = result.Errors.OfType<TError>().FirstOrDefault(e => e.Message.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase));
        error.ShouldNotBeNull(customMessage ?? $"Expected result to contain error of type {typeof(TError).Name} with message '{expectedMessage}', but found: {FormatErrors(result)}");
        return error;
    }

    #endregion

    #region Helpers

    private static string FormatErrors(IResultBase result)
    {
        if (!result.Errors.Any())
        {
            return "<no errors>";
        }

        return string.Join(", ", result.Errors.Select(e => $"[{e.GetType().Name}] {e.Message}"));
    }

    #endregion
}