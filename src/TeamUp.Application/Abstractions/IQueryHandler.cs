using MediatR;

namespace TeamUp.Application.Abstractions;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
	public new Task<TResponse> Handle(TQuery query, CancellationToken ct);
}
