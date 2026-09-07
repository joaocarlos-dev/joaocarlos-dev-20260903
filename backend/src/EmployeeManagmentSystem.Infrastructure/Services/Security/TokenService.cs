using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagmentSystem.Infrastructure.Services.Security;

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = GetValidatedOptions(options);

    public string Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(TokenClaims.SecurityVersion, user.SecurityVersion.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            now.AddMinutes(_options.ExpirationMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static JwtOptions GetValidatedOptions(IOptions<JwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Value.Validate();
        return options.Value;
    }
}

public static class TokenServiceDependencyInjection
{
    public static IServiceCollection AddTokenService(this IServiceCollection services)
    {
        services.AddSingleton<ITokenService, TokenService>();
        return services;
    }
}
