using GPA.Dtos.Security;
using GPA.Services.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GPA.Business.Security
{
    public interface IGPAJwtService
    {
        string? GenerateToken(TokenDescriptorDto tokenDescriptorDto);
    }

    public class GPAJwtService : IGPAJwtService
    {
        private readonly JwtOptions _jwtOptions;
        public GPAJwtService(JwtOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        public string? GenerateToken(TokenDescriptorDto tokenDescriptorDto)
        {
            if (tokenDescriptorDto is null || _jwtOptions is null)
            {
                return null;
            }

            var key = Encoding.UTF8.GetBytes(_jwtOptions.Key);
            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(tokenDescriptorDto.Claims),
                Expires = DateTime.Now.AddMinutes(_jwtOptions.Expires),
                Issuer = _jwtOptions.Issuer!,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), tokenDescriptorDto.Algorithm)
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);
            var token = handler.WriteToken(securityToken);
            return token;
        }
    }
}
