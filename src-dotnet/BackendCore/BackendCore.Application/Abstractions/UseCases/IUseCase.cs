namespace BackendCore.BackendCore.Application.Abstractions.UseCases;

public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
