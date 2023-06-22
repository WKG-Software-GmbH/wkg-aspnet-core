namespace Wkg.AspNetCore.RequestActions;

public delegate Task RequestTask();

public delegate Task<TResult> RequestTask<TResult>();