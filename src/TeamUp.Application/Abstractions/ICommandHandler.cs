using MediatR;

namespace TeamUp.Application.Abstractions;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
	public new Task<TResponse> Handle(TCommand command, CancellationToken ct);
};
