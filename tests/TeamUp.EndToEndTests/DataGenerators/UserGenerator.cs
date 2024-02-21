using TeamUp.Contracts.Users;

namespace TeamUp.EndToEndTests.DataGenerators;

public sealed class UserGenerator : BaseGenerator
{
	public static string GenerateValidPassword() => F.Internet.Password(10);

	public static readonly Faker<RegisterUserRequest> ValidRegisterUserRequest = new Faker<RegisterUserRequest>()
		.RuleFor(r => r.Email, f => f.Internet.Email())
		.RuleFor(r => r.Name, f => f.Internet.UserName())
		.RuleFor(r => r.Password, GenerateValidPassword());

	private static readonly Faker<User> EmptyUser = new Faker<User>()
		.UsePrivateConstructor()
		.RuleFor(u => u.Id, f => UserId.FromGuid(f.Random.Guid()))
		.RuleFor(u => u.Email, f => f.Internet.Email())
		.RuleFor(u => u.Name, f => f.Internet.UserName());

	public static readonly Faker<User> ActivatedUser = EmptyUser
		.RuleFor(u => u.Password, new Password())
		.RuleFor(u => u.Status, UserStatus.Activated);

	public static readonly Faker<User> NotActivatedUser = EmptyUser
		.RuleFor(u => u.Password, new Password())
		.RuleFor(u => u.Status, UserStatus.NotActivated);

	public static User GenerateUser(Password password, UserStatus status)
	{
		return EmptyUser
			.RuleFor(u => u.Password, password)
			.RuleFor(u => u.Status, status)
			.Generate();
	}

	public sealed class InvalidRegisterUserRequests : TheoryData<InvalidRequest<RegisterUserRequest>>
	{
		public InvalidRegisterUserRequests()
		{
			this.Add(x => x.Email, new RegisterUserRequest
			{
				Email = "invalid email",
				Name = F.Internet.UserName(),
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Name, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = "xx",
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Name, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = F.Lorem.Random.AlphaNumeric(User.NAME_MAX_SIZE + 1),
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Password, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = F.Internet.UserName(),
				Password = ""
			});
		}
	}
}
