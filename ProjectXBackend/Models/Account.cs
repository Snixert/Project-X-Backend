﻿using System;
using System.Collections.Generic;

namespace ProjectXBackend.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; }
    public DateTime TokenExpires { get; set; }

    public int? PlayerId { get; set; }

    public virtual Player? Player { get; set; }
}
