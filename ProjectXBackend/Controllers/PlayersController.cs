using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProjectXBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : Controller
    {
        private ApiDbContext dbContext;
        public PlayersController(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await dbContext.Players
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    WeaponId = p.WeaponId,
                    Level = p.Level,
                    Currency = p.Currency,
                    Weapon = new
                    {
                        Type = p.Weapon.Type,
                        Name = p.Weapon.Name,
                        ItemStats = p.Weapon.ItemStats.Select(w => new
                        {
                            StatsId = w.StatsId,
                            StatName = w.Stats.StatName,
                            StatsValue = w.StatsValue
                        }),
                    },
                    PlayerStats = p.PlayerStats.Select(ps => new
                    {
                        StatsId = ps.StatsId,
                        StatsValue = ps.StatsValue,
                        StatName = ps.Stats.StatName
                    }),
                    Items = p.Items.Select(i => new
                    {
                        Id = i.Id,
                        Type = i.Type,
                        Name = i.Name,
                        ItemStats = i.ItemStats.Select(f => new
                        {
                            StatsId = f.StatsId,
                            StatName = f.Stats.StatName,
                            StatValue = f.StatsValue
                        })
                    })
                }).ToListAsync();
            return Ok(players);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var players = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    WeaponId = p.WeaponId,
                    Level = p.Level,
                    Currency = p.Currency,
                    Weapon = new
                    {
                        Type = p.Weapon.Type,
                        Name = p.Weapon.Name,
                        ItemStats = p.Weapon.ItemStats.Select(w => new
                        {
                            StatsId = w.StatsId,
                            StatName = w.Stats.StatName,
                            StatsValue = w.StatsValue
                        }),
                    },
                    PlayerStats = p.PlayerStats.Select(ps => new
                    {
                        StatsId = ps.StatsId,
                        StatsValue = ps.StatsValue,
                        StatName = ps.Stats.StatName
                    }),
                    Items = p.Items.Select(i => new
                    {
                        Id = i.Id,
                        Type = i.Type,
                        Name = i.Name,
                        ItemStats = i.ItemStats.Select(f => new
                        {
                            StatsId = f.StatsId,
                            StatName = f.Stats.StatName,
                            StatValue = f.StatsValue
                        })
                    })
                }).FirstOrDefaultAsync();
            return Ok(players);
        }

        [HttpGet]
        [Route("{id:int}/inventory")]
        public async Task<IActionResult> GetPlayerInventory(int id)
        {
            var playerInventory = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Inventory = p.Items.Select(i => new
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Type = i.Type,
                        ItemStats = i.ItemStats.Select(stats => new
                        {
                            StatsId = stats.StatsId,
                            StatName = stats.Stats.StatName,
                            StatsValue = stats.StatsValue
                        })

                    })
                }).FirstOrDefaultAsync();
            return Ok(playerInventory);
        }

        [HttpGet]
        [Route("{id:int}/stats")]
        public async Task<IActionResult> GetPlayerStats(int id)
        {
            var playerStats = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    PlayerStats = p.PlayerStats.Select(stats => new
                    {
                        StatsId = stats.StatsId,
                        StatName = stats.Stats.StatName,
                        StatValue = stats.StatsValue
                    })
                }).FirstOrDefaultAsync();
            return Ok(playerStats);
        }
    }
}
