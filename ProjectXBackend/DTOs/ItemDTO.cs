namespace ProjectXBackend.DTOs
{
    public class ItemDTO
    {
        public int ItemId { get; set; }
        public int Type { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public ICollection<ItemStatDTO> ItemStats { get; set; } = new List<ItemStatDTO>();
    }
}
