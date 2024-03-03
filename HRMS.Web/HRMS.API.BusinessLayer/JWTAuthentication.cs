using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer.ITF;
using HRMS.Models.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRMS.API.BusinessLayer
{
    public class JWTAuthentication : IJWTAuthentication
    {
        public IConfiguration _config { get; set; }
        IBusinessLayer _businessLayer;
        IDataLayer _dataLayer;
        public JWTAuthentication(IConfiguration config, IBusinessLayer businessLayer, IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
            _dataLayer._configuration = config;
            _config = config;
            _businessLayer = businessLayer;
        }

        public LoginUser Authenticate(LoginUser model)
        {
            var tokenString = GenerateJSONWebToken(model);
            model = _businessLayer.LoginUser(model);
            if (model != null && string.IsNullOrEmpty(model.Result))
            {
                model.token = tokenString;
                return model;
            }
            return model;
        }


        private string GenerateJSONWebToken(LoginUser model)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, model.Email),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            authClaims.Add(new Claim(ClaimTypes.Role, model.Role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Issuer"],
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(authClaims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }


        private static IEnumerable<Claim> GetTokenClaims(LoginUser user)
        {
            return new List<Claim>
        {
        new Claim("UserName", user.Email),
        new Claim("Role", user.Role),
        //More custom claims
        };
        }
    }
}
