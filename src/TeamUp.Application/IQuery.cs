using MediatR;

namespace TeamUp.Application;

public interface IQuery<TResponse> : IRequest<TResponse>;
