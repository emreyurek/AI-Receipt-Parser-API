using Microsoft.AspNetCore.Mvc;
using ReceiptParserAPI.Data;
using ReceiptParserAPI.Dto;
using ReceiptParserAPI.Services;

namespace ReceiptParserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ReceiptDbContext _context;
        private readonly IPasswordHasherService _hasher;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public AuthController(ReceiptDbContext context, IPasswordHasherService hasher, IConfiguration configuration, IAuthService authService)
        {
            _context = context;
            _hasher = hasher;
            _configuration = configuration;
            _authService = authService;
        }
       
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto model)
        {
            var isRegistered = await _authService.RegisterAsync(model.Username, model.Email, model.Password);

            if (!isRegistered)
            {
                return BadRequest("Kullanıcı adı veya e-posta zaten kullanımda.");
            }

            return Ok(new { success = true, message = "Kayıt başarılı." });
        }
       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var result = await _authService.LoginAsync(model.Username, model.Password);

            if (result == null)
            {
                return Unauthorized(new { message = "Kullanıcı adı veya parola hatalı." });
            }

            return Ok(new
            {
                success = true,
                userId = result.Value.UserId,
                username = model.Username,
                token = result.Value.Token
            });
        }
    }
}
