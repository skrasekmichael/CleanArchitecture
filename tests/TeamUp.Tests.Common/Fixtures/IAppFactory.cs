using Microsoft.AspNetCore.Mvc.Testing;

namespace TeamUp.Tests.Common.Fixtures;

public interface IAppFactory<TSelf> where TSelf : WebApplicationFactory<Program>, IAppFactory<TSelf>
{
	public static abstract string HttpsPort { get; }
	public static abstract TSelf Create(string connectionString);
}
