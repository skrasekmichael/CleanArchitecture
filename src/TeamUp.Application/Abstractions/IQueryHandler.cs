using Mediato.Abstractions;

namespace TeamUp.Application.Abstractions;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
	public new Task<TResponse> HandleAsync(TQuery query, CancellationToken ct);
}
