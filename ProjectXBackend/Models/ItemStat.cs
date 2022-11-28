using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class ItemStat
{
    public int ItemId { get; set; }

    public int StatsId { get; set; }

    public int StatsValue { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Stat Stats { get; set; } = null!;
}
