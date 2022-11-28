using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? PlayerId { get; set; }

    public virtual Player? Player { get; set; }
}
