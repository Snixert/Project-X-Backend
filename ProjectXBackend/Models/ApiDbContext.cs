using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectXBackend.Models;

public partial class ApiDbContext : DbContext
{
    public ApiDbContext()
    {
    }

    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemStat> ItemStats { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerStat> PlayerStats { get; set; }

    public virtual DbSet<Stat> Stats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC27663D041C");

            entity.ToTable("Account");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Player).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_Account_PlayerID");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Items__3214EC270BC083D1");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ItemStat>(entity =>
        {
            entity.HasKey(e => new { e.ItemId, e.StatsId }).HasName("PK__ItemStat__5E5D221F0B28CF9E");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.StatsId).HasColumnName("StatsID");

            entity.HasOne(d => d.Item).WithMany(p => p.ItemStats)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemStats_ItemID");

            entity.HasOne(d => d.Stats).WithMany(p => p.ItemStats)
                .HasForeignKey(d => d.StatsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemStats_StatsID");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Player__3214EC27E050EAF2");

            entity.ToTable("Player");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.WeaponId).HasColumnName("WeaponID");

            entity.HasOne(d => d.Weapon).WithMany(p => p.PlayersNavigation)
                .HasForeignKey(d => d.WeaponId)
                .HasConstraintName("FK_Player_WeaponID");

            entity.HasMany(d => d.Items).WithMany(p => p.Players)
                .UsingEntity<Dictionary<string, object>>(
                    "Inventory",
                    r => r.HasOne<Item>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Inventory__ItemI__440B1D61"),
                    l => l.HasOne<Player>().WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Inventory__Playe__44FF419A"),
                    j =>
                    {
                        j.HasKey("PlayerId", "ItemId").HasName("PK__Inventor__ED699C961A7DA9D0");
                        j.ToTable("Inventory");
                    });
        });

        modelBuilder.Entity<PlayerStat>(entity =>
        {
            entity.HasKey(e => new { e.PlayerId, e.StatsId }).HasName("PK__PlayerSt__666DD55C0B93FF20");

            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
            entity.Property(e => e.StatsId).HasColumnName("StatsID");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerStats)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlayerStats_PlayerID");

            entity.HasOne(d => d.Stats).WithMany(p => p.PlayerStats)
                .HasForeignKey(d => d.StatsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlayerStats_StatsID");
        });

        modelBuilder.Entity<Stat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stats__3214EC27090BC7DF");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Stat1)
                .HasMaxLength(50)
                .HasColumnName("Stat");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
