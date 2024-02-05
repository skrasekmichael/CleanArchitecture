using MediatR;

namespace TeamUp.Application;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
	where TQuery : IQuery<TResponse>;
