using MediatR;

namespace TeamUp.Application;

public interface ICommand : IRequest;

public interface ICommand<TResponse> : IRequest<TResponse>;
