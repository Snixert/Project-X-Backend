using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

    public int? PlayerId { get; set; }

    public virtual Player? Player { get; set; }
}
