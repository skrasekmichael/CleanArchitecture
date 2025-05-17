using Mediato.Abstractions;

namespace TeamUp.Application.Abstractions;

public interface ICommand<TResponse> : IRequest<TResponse>;
