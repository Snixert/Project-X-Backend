using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProjectXBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : Controller
    {
        private ApiDbContext dbContext;
        public InventoryController(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [Route("{id:int}")]
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
        [Route("{playerId:int}")]
        public async Task<IActionResult> AddPlayerInventoryItem(int itemId, int playerId)
        {
            // Load player and player's inventory
            var player = await dbContext.Players.Include(p => p.InventorySlots).FirstOrDefaultAsync(p => p.Id == playerId);

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

        [HttpDelete]
        [Route("{playerId:int}")]
        public async Task<IActionResult> RemovePlayerInventoryItem(int playerId, int itemId)
        {
            // Load player and player's inventory
            var inventory = await dbContext.Inventory.Where(i => i.PlayerId == playerId).ToListAsync();

            // Check if inventory could be found.
            if (inventory is null)
            {
                return NotFound($"Inventory for player with Id = {playerId} could not be found.");
            }

            // Remove items with the requested ItemId
            foreach (var inventoryItem in inventory)
            {
                //Note; This way of removing all items needs to be changed if we define inventory SlotId.
                if (inventoryItem.ItemId == itemId)
                {
                    dbContext.Inventory.Remove(inventoryItem);
                }
            }

            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
