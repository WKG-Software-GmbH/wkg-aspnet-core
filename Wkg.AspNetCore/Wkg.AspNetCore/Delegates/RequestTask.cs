namespace Wkg.AspNetCore.Delegates;

/// <summary>
/// A delegate that represents an asynchronous API request action.
/// </summary>
/// <returns>A <see cref="Task"/> that can be used to retrieve the result of the asynchronous request action.</returns>
public delegate Task RequestTask();

/// <summary>
/// A delegate that represents an asynchronous API request action.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <returns>A <see cref="Task{TResult}"/> that can be used to retrieve the result of the asynchronous request action.</returns>
public delegate Task<TResult> RequestTask<TResult>();