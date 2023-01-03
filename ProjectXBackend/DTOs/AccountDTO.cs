using System.ComponentModel.DataAnnotations;

namespace ProjectXBackend.DTOs
{
    public class AccountDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
