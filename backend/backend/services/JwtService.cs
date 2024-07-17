using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using backend.results.db;
using backend.utils;
using Microsoft.IdentityModel.Tokens;

namespace backend.services;

public class JwtService(string? secret)
{
    public string GenerateToken(UserSchema user)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id!),
                new(ClaimTypes.Name, user.Username!)
            };

            var jwtToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret!)
                    ), SecurityAlgorithms.HmacSha256Signature));

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public string GetUserId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            if (securityToken == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "Invalid token");
            }

            var userIdClaim = securityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "User ID claim not found");
            }

            return userIdClaim.Value;
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public string GetUserName(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

            if (securityToken == null)
            {
                throw new ArgumentException("Invalid token");
            }

            var userNameClaim = securityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);

            if (userNameClaim == null)
            {
                throw new RestException(HttpStatusCode.Unauthorized, "Username claim not found");
            }

            return userNameClaim.Value;
        }
        catch (RestException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new RestException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}