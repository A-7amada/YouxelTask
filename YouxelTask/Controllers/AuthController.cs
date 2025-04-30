using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using YouxelTask.FileStorage.Api.Models;
namespace YouxelTask.FileStorage.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private static List<RefreshToken> refreshTokens = new List<RefreshToken>();
		private readonly IConfiguration _configuration;
		public AuthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		[HttpPost("login")]
		public IActionResult Login([FromBody] UserLogin userLogin)
		{
			var userName= _configuration["Jwt:UserName"];
			var password = _configuration["Jwt:Password"];
			// Validate the user credentials here (omitted for brevity)
			if (userLogin.Username != userName || userLogin.Password != password)
			{
				return Unauthorized();
			}

			var tokenString = GenerateJwtToken(userLogin.Username);
			var refreshToken = GenerateRefreshToken(userLogin.Username);

			return Ok(new
			{
				Token = tokenString,
				RefreshToken = refreshToken.Token
			});
		}

		[HttpPost("refresh")]
		public IActionResult Refresh([FromBody] TokenRequest tokenRequest)
		{
			var principal = GetPrincipalFromExpiredToken(tokenRequest.Token);
			if (principal == null)
			{
				return BadRequest("Invalid token");
			}

			var username = principal.Identity.Name;
			var storedRefreshToken = refreshTokens.Find(rt => rt.Username == username && rt.Token == tokenRequest.RefreshToken);

			if (storedRefreshToken == null || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
			{
				return Unauthorized("Invalid refresh token");
			}

			var newJwtToken = GenerateJwtToken(username);
			var newRefreshToken = GenerateRefreshToken(username);

			refreshTokens.Remove(storedRefreshToken);
			refreshTokens.Add(newRefreshToken);

			return Ok(new
			{
				Token = newJwtToken,
				RefreshToken = newRefreshToken.Token
			});
		}

		private string GenerateJwtToken(string username)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var secretKey = _configuration["Jwt:SecretKey"];
			var key = Encoding.ASCII.GetBytes(secretKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
				new Claim(ClaimTypes.Name, username)
				}),
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		private RefreshToken GenerateRefreshToken(string username)
		{
			var refreshToken = new RefreshToken
			{
				Token = Guid.NewGuid().ToString(),
				Username = username,
				ExpiryDate = DateTime.UtcNow.AddDays(7)
			};

			refreshTokens.Add(refreshToken);

			return refreshToken;
		}

		private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("your_secret_key");
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = false // Ignore expiration
			};

			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new SecurityTokenException("Invalid token");
			}

			return principal;
		}
	}

}
