using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Dtos
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
