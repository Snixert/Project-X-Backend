using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Item
{
    public int Id { get; set; }

    public int Type { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ItemStat> ItemStats { get; } = new List<ItemStat>();

    public virtual ICollection<Player> PlayersNavigation { get; } = new List<Player>();

    public virtual ICollection<Player> Players { get; } = new List<Player>();
    public virtual ICollection<InventorySlot> InventorySlots { get; } = new List<InventorySlot>();
}
