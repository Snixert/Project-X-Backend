using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectXBackend.DTOs;

namespace ProjectXBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly ApiDbContext dbContext;
        public ItemsController(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            List<ItemDTO> items = new List<ItemDTO>();
            var data = await dbContext.Items.Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).ToListAsync();

            foreach (var item in data)
            {
                ItemDTO dto = new ItemDTO();
                dto.ItemId = item.Id;
                dto.Name = item.Name;
                dto.Type = item.Type;
                foreach (var itemStat in item.ItemStats)
                {
                    ItemStatDTO isd = new ItemStatDTO();
                    isd.StatsId = itemStat.StatsId;
                    isd.StatName = itemStat.Stats.StatName;
                    isd.StatsValue = itemStat.StatsValue;

                    dto.ItemStats.Add(isd);
                }
                items.Add(dto);
            }

            return Ok(items);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await dbContext.Items.Where(x => x.Id == id).Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).FirstOrDefaultAsync();

            if (item is null)
            {
                return NotFound($"Item with the ID = {id} could not be found.");
            }

            ItemDTO dto = new ItemDTO();
            dto.ItemId = item.Id;
            dto.Name = item.Name;
            dto.Type = item.Type;
            foreach (var itemStat in item.ItemStats)
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
        [Route("{id:int}/inventory")]
        public async Task<IActionResult> GetPlayerInventory(int id)
        {
            var player = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Inventory = p.InventorySlots.Select(items => new
                    {
                        ItemId = items.ItemId,
                        ItemName = items.Item.Name,
                        ItemType = items.Item.Type,
                        ItemStats = items.Item.ItemStats.Select(s => new
                        {
                            StatsId = s.StatsId,
                            StatName = s.Stats.StatName,
                            StatValue = s.StatsValue
                        }),
                    }),
                }).FirstOrDefaultAsync();

            if (player is null)
            {
                return NotFound($"Player with Id = {id} could not be found");
            }
            return Ok(player);
        }

        [HttpPost]
        [Route("{playerId:int}/additem")]
        public async Task<IActionResult> AddPlayerInventoryItem(int itemId, int playerId)
        {
            // Load player and player's inventory
            var player = await dbContext.Players.Include(p => p.InventorySlots).FirstOrDefaultAsync(x => x.Id == playerId);

            // Check if player exists
            if (player is null)
            {
                return NotFound($"Player with Id = {playerId} could not be found.");
            }

            // Check if player has room in inventory
            if (player.InventorySlots.Count >= 16)
            {
                return Conflict($"Inventory is full for player with Id = {playerId}");
            }

            InventorySlot item = new InventorySlot()
            {
                PlayerId = playerId,
                ItemId = itemId
            };

            player.InventorySlots.Add(item);
            await dbContext.SaveChangesAsync();
            return Ok(player.InventorySlots);
        }
    }
}
