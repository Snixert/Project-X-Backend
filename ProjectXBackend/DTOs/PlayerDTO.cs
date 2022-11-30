using ProjectXBackend.Models;

namespace ProjectXBackend.DTOs
{
    public class PlayerDTO
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public int Currency { get; set; }
        public Item? Weapon { get; set; }
        public ICollection<PlayerStatDTO> PlayerStats { get; set; } = new List<PlayerStatDTO>();
    }
}
