using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class PlayerStat
{
    public int PlayerId { get; set; }

    public int StatsId { get; set; }

    public int StatsValue { get; set; }

    public virtual Player Player { get; set; } = null!;

    public virtual Stat Stats { get; set; } = null!;
}