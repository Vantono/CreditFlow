using CreditFlowAPI.Base.Identity;
using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Domain.Entities;
using CreditFlowAPI.Feature.Loans.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest dto)
        {
            // 1. Ελέγχουμε αν υπάρχει ήδη
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest("Το Email χρησιμοποιείται ήδη.");

            // 2. Φτιάχνουμε τον χρήστη
            var user = new ApplicationUser
            {
                UserName = dto.Email, // Στο Identity το UserName είναι συνήθως το Email
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TaxId = dto.TaxId,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode
            };

            // 3. Τον αποθηκεύουμε (Το Identity κάνει Hash το password μόνο του!)
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            // 4. (Προαιρετικά) Του δίνουμε ρόλο "Customer"
            // await _userManager.AddToRoleAsync(user, "Customer");

            return Ok(new { message = "Η εγγραφή ολοκληρώθηκε!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest dto)
        {
            // 1. Βρίσκουμε τον χρήστη
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Λάθος email ή κωδικός.");

            // 2. Ελέγχουμε τον κωδικό
            var result = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!result) return Unauthorized("Λάθος email ή κωδικός.");

            // 3. Παράγουμε το Token
            var token = await _tokenService.CreateTokenAsync(user);

            // 4. Επιστρέφουμε Token και βασικά στοιχεία
            return Ok(new
            {
                token = token,
                email = user.Email,
                fullName = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}
