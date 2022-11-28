using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Stat
{
    public int Id { get; set; }

    public string StatName { get; set; } = null!;

    public virtual ICollection<ItemStat> ItemStats { get; } = new List<ItemStat>();

    public virtual ICollection<PlayerStat> PlayerStats { get; } = new List<PlayerStat>();
}
