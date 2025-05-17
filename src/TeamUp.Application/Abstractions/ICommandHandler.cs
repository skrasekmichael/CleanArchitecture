using Mediato.Abstractions;

namespace TeamUp.Application.Abstractions;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
	public new Task<TResponse> HandleAsync(TCommand command, CancellationToken ct);
};
