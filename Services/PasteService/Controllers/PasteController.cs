using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasteService.Data;
using PasteService.Models;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace PasteService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PasteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Paste
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Paste>>> GetPastes()
        {
            return await _context.Pastes
                .Where(p => p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        // GET: api/Paste/5
        [HttpGet("{code}")]
        public async Task<ActionResult<Paste>> GetPaste(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == code);

            if (paste == null)
            {
                return NotFound();
            }

            if (paste.ExpiresAt.HasValue && paste.ExpiresAt.Value < DateTime.UtcNow)
            {
                return NotFound();
            }

            if (paste.Visibility == "private")
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized();
                }

                var currentUserId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                if (paste.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            paste.ViewCount++;
            await _context.SaveChangesAsync();

            return paste;
        }

        // GET: api/Paste/mine
        [Authorize]
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<Paste>>> GetMine()
        {
            var currentUserId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            return await _context.Pastes
                .Where(p => p.OwnerId == currentUserId)
                .ToListAsync();
        }

        // POST: api/Paste
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Paste>> PostPaste(CreatePasteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Content cannot be empty.");
            }
            if (Encoding.UTF8.GetByteCount(request.Content) > 500 * 1024)
            {
                return BadRequest("Content exceeds the maximum allowed size of 500 KB.");
            }
            DateTime? expiresAt = request.Expiry switch
            {
                "1h" => DateTime.UtcNow.AddHours(1),
                "1d" => DateTime.UtcNow.AddDays(1),
                "1w" => DateTime.UtcNow.AddDays(7),
                _ => null,
            };
            string code;
            do
            {
                code = GenerateCode();
            } while (await _context.Pastes.AnyAsync(p => p.Code == code));

            int? ownerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                ownerId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            }

            var paste = new Paste
            {
                Code = code,
                Content = request.Content,
                Language = request.Language,
                Visibility = request.Visibility,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                OwnerId = ownerId
            };
            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaste", new { code = paste.Code }, paste);
        }

        // DELETE: api/Paste/5
        [HttpDelete("{code}")]
        public async Task<IActionResult> DeletePaste(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == code);
            if (paste == null)
            {
                return NotFound();
            }

            if (paste.OwnerId.HasValue)
            {
                if(User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized();
                }

                var currentUserId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
                if (paste.OwnerId != currentUserId)
                {
                    return Forbid();
                }
            }

            _context.Pastes.Remove(paste);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static string GenerateCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var codeChars = new char[length];

            for(int i = 0; i < length; i++)
            {
                codeChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(codeChars);
        }
    }

    public class CreatePasteRequest
{
    public string Content {get; set;}
    public string Language {get; set;}
    public string Visibility{get; set;}
    public string Expiry {get; set;}
    
}
}


