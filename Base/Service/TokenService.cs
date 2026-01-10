using CreditFlowAPI.Base.Identity;
using Microsoft.AspNetCore.Identity; // Χρειάζεται για τον UserManager
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CreditFlowAPI.Base.Service
{
    public interface ITokenService
    {
        // Αλλαγή σε Task<string> γιατί πλέον μιλάμε με τη βάση για τους ρόλους
        Task<string> CreateTokenAsync(ApplicationUser user);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<ApplicationUser> _userManager; // Νέο dependency

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            // Διαβάζουμε το μυστικό κλειδί από το appsettings.json
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        }

        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            // 1. Δημιουργούμε τα βασικά Claims (User Info)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // UserID
                new Claim(JwtRegisteredClaimNames.Email, user.Email!), // Email
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? ""), // Όνομα
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? ""), // Επίθετο
                new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                
                // Σημαντικό: Το JTI (JWT ID) βοηθάει στην μοναδικότητα του token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 2. ΝΕΟ: Φέρνουμε τους Ρόλους από τη βάση και τους προσθέτουμε
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                // Το σύστημα Authorization περιμένει το ClaimTypes.Role
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Υπογράφουμε το Token
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // 4. Ορίζουμε τις παραμέτρους
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:DurationInMinutes"]!)),
                SigningCredentials = creds,
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            // 5. Παράγουμε το Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}