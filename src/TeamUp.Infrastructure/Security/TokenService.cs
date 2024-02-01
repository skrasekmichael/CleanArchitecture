using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TeamUp.Application.Users;
using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Infrastructure.Identity;

internal sealed class TokenService : ITokenService
{
	private readonly ILogger<TokenService> _logger;
	private readonly IOptions<JwtOptions> _jwtSettings;
	private readonly IDateTimeProvider _dateTimeProvider;

	public TokenService(ILogger<TokenService> logger, IOptions<JwtOptions> jwtSettings, IDateTimeProvider dateTimeProvider)
	{
		_logger = logger;
		_jwtSettings = jwtSettings;
		_dateTimeProvider = dateTimeProvider;
	}

	public string GenerateToken(User user)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Value.Issuer,
			audience: _jwtSettings.Value.Audience,
			notBefore: _dateTimeProvider.UtcNow.AddMinutes(0),
			expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.Value.ExpirationMinutes),
			signingCredentials: GetSigningCredentials(),
			claims: GetClaims(user)
		);

		_logger.LogInformation("JWT Created");
		return tokenHandler.WriteToken(token);
	}

	private List<Claim> GetClaims(User user)
	{
		try
		{
			return [
				new(JwtRegisteredClaimNames.Sub, user.Email),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.Name),
				new(ClaimTypes.Email, user.Email)
			];
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when generating JWT Claims.");
			throw;
		}
	}

	private SigningCredentials GetSigningCredentials()
	{
		var key = Encoding.UTF8.GetBytes(_jwtSettings.Value.SigningKey);
		var symmetricSecurityKey = new SymmetricSecurityKey(key);
		return new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512);
	}
}
