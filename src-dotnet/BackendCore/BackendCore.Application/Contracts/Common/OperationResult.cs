namespace BackendCore.BackendCore.Application.Contracts.Common;

public sealed record OperationResult(bool IsSuccess, string? Error = null)
{
    public static OperationResult Success() => new(true);

    public static OperationResult Failure(string error) => new(false, error);
}

public sealed record OperationResult<T>(bool IsSuccess, T? Data = default, string? Error = null)
{
    public static OperationResult<T> Success(T data) => new(true, data, null);

    public static OperationResult<T> Failure(string error) => new(false, default, error);
}
