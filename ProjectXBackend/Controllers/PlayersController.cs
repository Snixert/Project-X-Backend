using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectXBackend.DTOs;

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
                    Inventory = p.InventorySlots.Select(i => new
                    {
                        Id = i.Item.Id,
                        Type = i.Item.Type,
                        Name = i.Item.Name,
                        ItemStats = i.Item.ItemStats.Select(f => new
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
                    Inventory = p.InventorySlots.Select(i => new
                    {
                        Id = i.Item.Id,
                        Type = i.Item.Type,
                        Name = i.Item.Name,
                        ItemStats = i.Item.ItemStats.Select(f => new
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
        [Route("{id:int}/weapon")]
        public async Task<IActionResult> GetPlayerWeapon(int id)
        {
            var player = await dbContext.Players
                .Include(p => p.Weapon)
                .ThenInclude(w => w.ItemStats)
                .ThenInclude(stats => stats.Stats)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (player is null || player.Weapon is null)
            {
                return new EmptyResult();
            }

            ItemDTO dto = new ItemDTO();
            dto.ItemId = player.Weapon.Id;
            dto.Name = player.Weapon.Name;
            dto.Type = player.Weapon.Type;
            foreach (var itemStat in player.Weapon.ItemStats)
            {
                ItemStatDTO isd = new ItemStatDTO();
                isd.StatsId = itemStat.StatsId;
                isd.StatName = itemStat.Stats.StatName;
                isd.StatsValue = itemStat.StatsValue;

                dto.ItemStats.Add(isd);
            }

            return Ok(dto);
        }

        [HttpGet]
        [Route("{id:int}/stats")]
        public async Task<IActionResult> GetPlayerStats(int id)
        {
            var playerStats = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    PlayerStats = p.PlayerStats.Select(stats => new
                    {
                        StatsId = stats.StatsId,
                        StatName = stats.Stats.StatName,
                        StatValue = stats.StatsValue
                    })
                }).FirstOrDefaultAsync();

            if (playerStats is null)
            {
                return NotFound($"Player with Id = {id} could not be found");
            }

            return Ok(playerStats);
        }

        [HttpPut]
        [Route("{id:int}/stats")]
        public async Task<IActionResult> UpdatePlayerStats([FromBody] List<PlayerStatDTO> updatePlayerStatRequest, int id)
        {
            // Load the Player and its PlayerStats
            var player = await dbContext.Players
                .Include(p => p.PlayerStats)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player is null)
            {
                return NotFound($"Player with the Id = {id} could not be found.");
            }

            // Delete PlayerStats
            foreach (var playerStat in player.PlayerStats)
            {
                if (!updatePlayerStatRequest.Any(ps => ps.StatsId == playerStat.StatsId))
                {
                    dbContext.PlayerStats.Remove(playerStat);
                }
            }

            foreach (var newPlayerStat in updatePlayerStatRequest)
            {
                var existingStats = player.PlayerStats
                    .Where(ps => ps.StatsId == newPlayerStat.StatsId && ps.StatsId != default(int))
                    .SingleOrDefault();

                // Update existing PlayerStats with request values
                if (existingStats != null)
                {
                    existingStats.StatsId = newPlayerStat.StatsId;
                    existingStats.StatsValue = newPlayerStat.StatsValue;

                    // Set PlayerStat state to modified so it gets saved to the Database
                    dbContext.Entry(existingStats).State = EntityState.Modified;
                }
                else
                {
                    // Insert a new PlayerStat
                    var newStat = new PlayerStat
                    {
                        StatsId = newPlayerStat.StatsId,
                        StatsValue = newPlayerStat.StatsValue,
                        PlayerId = player.Id,
                    };
                    player.PlayerStats.Add(newStat);
                }
            }
            await dbContext.SaveChangesAsync();
            return Ok(player);
        }
    }
}
