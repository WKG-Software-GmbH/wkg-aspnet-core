using System.Diagnostics.CodeAnalysis;

namespace Wkg.AspNetCore.Abstractions.Managers.Results;

/// <summary>
/// Represents the result of a manager operation with a return value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public readonly struct ManagerResult<TResult>
{
    private readonly ManagerResult _inner;
    private readonly TResult _result;

    internal ManagerResult(ManagerResult inner, TResult result)
    {
        _inner = inner;
        _result = result;
    }

    /// <inheritdoc cref="ManagerResult.StatusCode"/>"
    public readonly ManagerResultCode StatusCode => _inner.StatusCode;

    /// <inheritdoc cref="ManagerResult.IsEmpty"/>"
    public readonly bool IsEmpty => _inner.IsEmpty;

    /// <inheritdoc cref="ManagerResult.IsSuccess"/>"
    public readonly bool IsSuccess => _inner.IsSuccess;

    /// <inheritdoc cref="ManagerResult.ErrorMessage"/>"
    public readonly string? ErrorMessage => _inner.ErrorMessage;

    /// <summary>
    /// Attempts to get the result of the operation.
    /// </summary>
    /// <param name="result">The result of the operation or <see langword="null"/> if the operation failed.</param>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryGetResult([NotNullWhen(true)] out TResult? result)
    {
        if (IsSuccess)
        {
            result = _result!;
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Attempts to get the result of the operation.
    /// </summary>
    /// <param name="status">The status code of the operation.</param>
    /// <param name="result">The result of the operation or <see langword="null"/> if the operation failed.</param>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryGetResult(out ManagerResultCode status, [NotNullWhen(true)] out TResult? result)
    {
        status = _inner.StatusCode;
        return TryGetResult(out result);
    }

    /// <summary>
    /// Gets the result of the operation or throws an exception if the operation failed.
    /// </summary>
    public TResult GetOrThrow() => IsSuccess ? _result! : throw new InvalidOperationException(_inner.ErrorMessage);

    /// <inheritdoc cref="ManagerResult.FailureAs{TOther}"/>"
    public readonly ManagerResult<TOther> FailureAs<TOther>() => _inner.FailureAs<TOther>();

    /// <summary>
    /// Returns a new <see cref="ManagerResult"/> with the same status code and error message as the current instance, but without a value.
    /// </summary>
    public readonly ManagerResult WithoutValue() => _inner;

    /// <summary>
    /// Converts the current instance to a <see cref="ManagerResult"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator ManagerResult(ManagerResult<TResult> result) => result._inner;
}