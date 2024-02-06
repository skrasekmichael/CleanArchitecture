using System.Runtime.CompilerServices;

namespace TeamUp.Common;

public abstract record ErrorBase
{
	public string Code { get; internal init; } = null!;
	public string Message { get; internal init; } = null!;
}

public abstract record Error<TSelf> : ErrorBase where TSelf : ErrorBase, new()
{
	public static TSelf New(string message, [CallerMemberName] string? caller = null, [CallerFilePath] string? filePath = null)
	{
		var className = Path.GetFileNameWithoutExtension(filePath);
		return new TSelf()
		{
			Code = $"{className}.{caller}",
			Message = message
		};
	}
}

public sealed record AuthenticationError : Error<AuthenticationError>;
public sealed record AuthorizationError : Error<AuthorizationError>;
public sealed record NotFoundError : Error<NotFoundError>;
public sealed record DomainError : Error<DomainError>;
public sealed record ValidationError : Error<ValidationError>;
public sealed record InternalError : Error<InternalError>;
