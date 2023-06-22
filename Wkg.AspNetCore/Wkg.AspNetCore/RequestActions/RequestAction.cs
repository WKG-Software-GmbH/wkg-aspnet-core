namespace Wkg.AspNetCore.RequestActions;

public delegate void RequestAction();

public delegate TResult RequestAction<out TResult>();