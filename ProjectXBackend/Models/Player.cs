using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Player
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string Image { get; set; }

    public int WeaponId { get; set; }

    public int? Level { get; set; }

    public int? Currency { get; set; }

    public virtual ICollection<Account> Accounts { get; } = new List<Account>();

    public virtual ICollection<PlayerStat> PlayerStats { get; set; } = new List<PlayerStat>();

    public virtual Item? Weapon { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<InventorySlot> InventorySlots { get; set; } = new List<InventorySlot>();
}
