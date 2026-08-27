using Cavista.CTRecruita.Data.Contexts;
using Cavista.CTRecruita.Data.Entities.Auth;
using Cavista.CTRecruita.Data.Entities.BaseEntites;
using Cavista.CTRecruita.Utilities.ApiResponse;
using Cavista.CTRecruita.Utilities.Mediator.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cavista.CTRecruita.Commands.Auth
{
    public class LoginCommand : IRequest<ApiResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
    public class AuthModel
    {
        public DateTime TokenExpiration { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
    }
    public class LoginHandler : IRequestHandler<LoginCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        public LoginHandler(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return new ApiResponse(true, (int)StatusCodes.Status401Unauthorized, "Invalid Username and Password");
            if (user.UserStatus != UserStatus.Active)
                return new ApiResponse(true, (int)StatusCodes.Status403Forbidden, "Account is not active");
            if (user.RequiresPasswordReset)
                return new ApiResponse(true, (int)StatusCodes.Status403Forbidden, "Password reset required");
            var loginToken = await GenerateJWTTokenAsync(user);
            return new ApiResponse(false, (int)StatusCodes.Status200OK, "Login successful", loginToken);
        }
        private async Task<AuthModel> GenerateJWTTokenAsync(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
           {
               new(JwtRegisteredClaimNames.Sub, user.Email!),
               new(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
               new(ClaimTypes.NameIdentifier, user.UserName ?? string.Empty),
               new(ClaimTypes.Email, user.Email!),
               new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
               new("UserId", user.Id.ToString())
           };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
            claims.AddRange(userClaims);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expire = DateTime.UtcNow.AddMinutes(
                int.TryParse(_configuration["JWT:ExpiryTime"], out var mins) ? mins : 5);
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Issuer"],
                claims: claims,
                expires: expire,
                signingCredentials: creds);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return new AuthModel
            {
                TokenExpiration = token.ValidTo,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Username = user.UserName!
            };
        }
    }
}
