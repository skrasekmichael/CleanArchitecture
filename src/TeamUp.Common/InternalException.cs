namespace TeamUp.Common;

public sealed class InternalException : Exception
{
	public InternalException(string message) : base(message) { }
}
