namespace ProjectXBackend.Models
{
    public class InventorySlot
    {
        public int PlayerId { get; set; }
        public int ItemId { get; set; }
        public virtual Player Player { get; set; } = null!;
        public virtual Item Item { get; set; } = null!;
    }
}
