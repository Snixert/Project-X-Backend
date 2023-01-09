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
            // This way of loading gives a similar result to using a DTO but has more risks of breaking due to typos etc.
            // I've kept it this way as mapping this into DTOs would require about 5 foreach loops and that'd give me motion sickness.
            var players = await dbContext.Players
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    WeaponId = p.WeaponId,
                    Level = p.Level,
                    Currency = p.Currency,
                    Weapon = new
                    {
                        Type = p.Weapon.Type,
                        Name = p.Weapon.Name,
                        Price = p.Weapon.Price,
                        Image = p.Weapon.Image,
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
            // This way of loading gives a similar result to using a DTO but has more risks of breaking due to typos etc.
            // I've kept it this way, it works and I cba making a DTO and mapping for this.
            var players = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.Image,
                    WeaponId = p.WeaponId,
                    Level = p.Level,
                    Currency = p.Currency,
                    Weapon = new
                    {
                        Type = p.Weapon.Type,
                        Name = p.Weapon.Name,
                        Price = p.Weapon.Price,
                        Image = p.Weapon.Image,
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
            // Load a player and include/theninclude the nested object properties.
            var player = await dbContext.Players
                .Include(p => p.Weapon)
                .ThenInclude(w => w.ItemStats)
                .ThenInclude(stats => stats.Stats)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            // Check if player exists and if the player somehow has no weapon (They should always have a weapon).
            if (player is null || player.Weapon is null)
            {
                return BadRequest($"Player with Id = {id} could not be found or the player somehow does not have a weapon");
            }

            // Map weapon properties to a DTO.
            ItemDTO dto = new ItemDTO();
            dto.ItemId = player.Weapon.Id;
            dto.Name = player.Weapon.Name;
            dto.Type = player.Weapon.Type;
            dto.Price = player.Weapon.Price;
            dto.Image = player.Weapon.Image;
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
            // Load PlayerStats from a player based on a PlayerID.
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

            // Check if player exists.
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

            foreach (var updatedPlayerStat in updatePlayerStatRequest)
            {
                // Get the PlayerStat from the player that has the same StatsID as updatedPlayerStat's ID
                // I do this to edit the existing stat on the player instead of removing/adding a whole PlayerStat object.
                var existingStats = player.PlayerStats
                    .Where(ps => ps.StatsId == updatedPlayerStat.StatsId && ps.StatsId != default(int))
                    .SingleOrDefault();

                // Update existing PlayerStats with request values
                if (existingStats != null)
                {
                    existingStats.StatsValue = updatedPlayerStat.StatsValue;

                    // Set PlayerStat state to modified so it gets saved to the Database (This is required for it to work for some reason).
                    dbContext.Entry(existingStats).State = EntityState.Modified;
                }
            }
            await dbContext.SaveChangesAsync();
            return Ok(player);
        }

        [HttpPut]
        [Route("{playerId:int}/weapon/{weaponId:int}")]
        public async Task<IActionResult> UpdatePlayerWeapon(int playerId, int weaponId)
        {
            // Load a player and Include the player's Inventory
            var player = await dbContext.Players.Where(x => x.Id == playerId).Include(p => p.InventorySlots).FirstOrDefaultAsync();

            // Check if item exists
            if (!dbContext.Items.Any(i => i.Id == weaponId))
            {
                return NotFound($"Item with Id = {weaponId} could not be found.");
            }

            // Check if player exists
            if (player is null)
            {
                return NotFound($"Player with Id = {playerId} could not be found.");
            }

            // Add the un-equipped item to inventory
            if (!player.InventorySlots.Any(i => i.ItemId == player.WeaponId))
            {
                InventorySlot oldItem = new InventorySlot()
                {
                    PlayerId = playerId,
                    ItemId = player.WeaponId
                };
                player.InventorySlots.Add(oldItem);
            }
            // Remove the newly equipped item from Inventory
            if (player.InventorySlots.Any(i => i.ItemId == weaponId))
            {
                foreach (var item in player.InventorySlots)
                {
                    if (item.ItemId == weaponId)
                    {
                        dbContext.Inventory.Remove(item);
                    }
                }
            }

            // Update the player's weapon
            player.WeaponId = weaponId;
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        [Route("{id:int}/level/{level:int}/currency/{currency:int}")]
        public async Task<IActionResult> UpdatePlayerLevelCurrency(int id, int level, int currency)
        {
            // Load a specific player
            // No need to use .Include and .ThenInclude as this will update properties on the Player class and not nested properties.
            var player = await dbContext.Players.Where(x => x.Id == id).FirstOrDefaultAsync();

            // Check if player exists
            if (player is null)
            {
                return NotFound($"Player with Id = {id} could not be found.");
            }

            // Note: Adding levels like this will probably have to be changed depending on how we want levels to work.
            // Add the parameter values to existing values.
            player.Level = player.Level + level;
            player.Currency = player.Currency + currency;

            await dbContext.SaveChangesAsync();
            return Ok();

        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer([FromBody] AddPlayerDTO addPlayerRequest)
        {
            var account = await dbContext.Accounts.Where(x => x.Id == addPlayerRequest.AccountId).FirstOrDefaultAsync();

            if (account is null)
            {
                return NotFound($"Account with Id = {addPlayerRequest.AccountId} could not be found.");
            }

            // Create player object
            Player playerCharacter = new Player();
            playerCharacter.Name = addPlayerRequest.Name;
            playerCharacter.Image = addPlayerRequest.Image;
            playerCharacter.Level = 1;
            playerCharacter.Currency = 0;
            // I'm setting a default value for weapon here as players need to have a weapon, change the ID to whatever.
            playerCharacter.WeaponId = 1;

            // Find all stats and add a PlayerStat for every stat in the DB
            List<PlayerStat> playerStats = new List<PlayerStat>();
            var stats = await dbContext.Stats.ToListAsync();
            foreach (var stat in stats)
            {
                // Set a default value of 5 for each stat
                PlayerStat pStat = new PlayerStat();
                pStat.StatsId = stat.Id;
                pStat.StatsValue = 5;
                pStat.PlayerId = playerCharacter.Id;

                playerCharacter.PlayerStats.Add(pStat);
            }
            account.Player = playerCharacter;
            await dbContext.SaveChangesAsync();
            return Ok();


        }
    }
}
