namespace TeamUp.Common;

public abstract record ErrorBase
{
	public string Code { get; protected init; } = null!;
	public string Message { get; protected init; } = null!;
}

public abstract record Error<TSelf> : ErrorBase where TSelf : Error<TSelf>, new()
{
	public static TSelf New(string message, string code = "")
	{
		return new TSelf()
		{
			Code = code,
			Message = message
		};
	}
}

public sealed record AuthenticationError : Error<AuthenticationError>;
public sealed record AuthorizationError : Error<AuthorizationError>;
public sealed record NotFoundError : Error<NotFoundError>;
public sealed record ConflictError : Error<ConflictError>;
public sealed record DomainError : Error<DomainError>;
public sealed record ValidationError : Error<ValidationError>;
public sealed record InternalError : Error<InternalError>;
