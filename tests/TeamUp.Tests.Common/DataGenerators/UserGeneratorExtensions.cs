namespace TeamUp.Tests.Common.DataGenerators;

public static class UserGeneratorExtensions
{
	public static Faker<User> WithStatus(this Faker<User> userGenerator, UserStatus status) => userGenerator.RuleFor(u => u.Status, status);

	public static Faker<User> WithPassword(this Faker<User> userGenerator, Password password) => userGenerator.RuleFor(u => u.Password, password);
}
