using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthService.Data;
using AuthService.Models;
using AuthService.Models.Dtos;

namespace AuthService.Controllers
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

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
        {
            var emailTaken = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailTaken)
            {
                return Conflict(new { message = "Email đã được sử dụng." });
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
            }

            var (token, expiresAt) = GenerateJwtToken(user);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshToken.Token,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        // POST: api/Auth/refresh
        // Cấp access token mới từ refresh token còn hiệu lực. Refresh token cũ bị thu hồi (rotation)
        // và một refresh token mới được cấp để thay thế.
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        {
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (existingToken == null || !existingToken.IsActive)
            {
                return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn." });
            }

            existingToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var (token, expiresAt) = GenerateJwtToken(existingToken.User);
            var newRefreshToken = await CreateRefreshTokenAsync(existingToken.UserId);

            var user = existingToken.User;
            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = newRefreshToken.Token,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }

        // POST: api/Auth/logout
        // Thu hồi refresh token ngay lập tức -> client không thể dùng nó để lấy access token mới nữa.
        // Đây là "logout thật sự" ở phía server, khác với việc client chỉ tự xoá token.
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequest request)
        {
            var existingToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (existingToken != null && existingToken.RevokedAt == null)
            {
                existingToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // GET: api/Auth/me
        // Dùng để kiểm tra token JWT có hợp lệ không (test "đang đăng nhập" vs "đã logout").
        [Authorize]
        [HttpGet("me")]
        public ActionResult<UserResponse> Me()
        {
            var response = new UserResponse
            {
                Id = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
                Username = User.FindFirstValue(ClaimTypes.Name)!,
                Email = User.FindFirstValue(JwtRegisteredClaimNames.Email)!,
                Role = User.FindFirstValue(ClaimTypes.Role)!
            };

            return Ok(response);
        }

        private async Task<RefreshToken> CreateRefreshTokenAsync(int userId)
        {
            var refreshTokenDays = double.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7");

            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiresInMinutes"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
