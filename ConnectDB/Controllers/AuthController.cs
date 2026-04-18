using BCrypt.Net;
using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConnectDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username đã tồn tại.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email đã được sử dụng.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = 0 // 0 = User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.Username == dto.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");

            var token = GenerateJwtToken(user);

            return Ok(new 
            { 
                Message = "Đăng nhập thành công",
                Token = token,
                User = new { 
                    user.UserId, 
                    user.Username, 
                    user.FullName, 
                    Role = user.Role?.RoleName 
                }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound("Không tìm thấy tài khoản với Email này.");

            var resetToken = Guid.NewGuid().ToString("N");
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            // Thực tế sẽ gửi email ở đây. Tạm thời mình trả thẳng token về FE để test
            return Ok(new 
            { 
                Message = "Token reset mật khẩu đã được tạo (Giả lập gửi qua Email)", 
                ResetToken = resetToken 
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == dto.Token);
            
            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Mật khẩu đã được khôi phục thành công." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            // Yêu cầu token mới đổi được mật khẩu
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Vui lòng đăng nhập.");

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim.Value));
            if (user == null) return NotFound("User không tồn tại.");

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest("Mật khẩu cũ không chính xác.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đổi mật khẩu thành công!" });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Vui lòng đăng nhập.");

            var user = await _context.Users.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.UserId == int.Parse(userIdClaim.Value));
            if (user == null) return NotFound("User không tồn tại.");

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FullName,
                Role = user.Role?.RoleName
            });
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.FullName,
                    Role = u.Role != null ? u.Role.RoleName : "User"
                })
                .ToListAsync();

            return Ok(users);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var keyElement = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(keyElement)) throw new Exception("Không tìm thấy Secret Key.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyElement));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
