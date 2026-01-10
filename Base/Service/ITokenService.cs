using CreditFlowAPI.Base.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CreditFlowAPI.Base.Service
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Διαβάζουμε το μυστικό κλειδί από το appsettings.json
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        }

        public string CreateToken(ApplicationUser user)
        {
            // 1. Δημιουργούμε τα Claims (τα στοιχεία που περιέχει το διαβατήριο)
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // To UserID
            new Claim(JwtRegisteredClaimNames.Email, user.Email!), // To Email
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""), // Όνομα
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""), // Επίθετο
            new Claim("FullName", $"{user.FirstName} {user.LastName}") // Βολικό για το Frontend
        };

            // 2. Υπογράφουμε το Token με το κλειδί μας
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 3. Ορίζουμε τις παραμέτρους (Ποιος το έβγαλε, για ποιον, πότε λήγει)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:DurationInMinutes"]!)),
                SigningCredentials = creds,
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            // 4. Παράγουμε το τελικό String
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
 }
