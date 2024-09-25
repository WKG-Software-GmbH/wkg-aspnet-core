namespace Wkg.AspNetCore.Delegates;

/// <summary>
/// A delegate that represents an API request action.
/// </summary>
public delegate void RequestAction();

/// <summary>
/// A delegate that represents an API request action.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public delegate TResult RequestAction<out TResult>();