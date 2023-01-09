using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
        [Route("{id:int}"), Authorize]
        public async Task<IActionResult> GetPlayerInventory(int id)
        {
            // This way of loading gives a similar result to using a DTO but has more risks of breaking due to typos etc.
            // This was planned to be using DTOs but I never got around to it.

            // Load just the inventory from a player with the given playerId
            var player = await dbContext.Players
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Inventory = p.InventorySlots.Select(items => new
                    {
                        ItemId = items.ItemId,
                        ItemName = items.Item.Name,
                        ItemType = items.Item.Type,
                        Image = items.Item.Image,
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
        [Route("{playerId:int}/add/{itemId:int}")]
        public async Task<IActionResult> AddPlayerInventoryItem(int itemId, int playerId)
        {
            // Load player and player's inventory
            var player = await dbContext.Players.Include(p => p.InventorySlots).FirstOrDefaultAsync(p => p.Id == playerId);

            // Load the Item to check for its price
            var shopItem = await dbContext.Items.Where(i => i.Id == itemId).FirstOrDefaultAsync();

            // Check if player and item exist
            if (player is null || shopItem is null)
            {
                return NotFound($"Unable to find either player with Id = {playerId} or item with Id = {itemId}");
            }

            // Check if player has enough currency to purchase item
            if (player.Currency < shopItem.Price)
            {
                return Problem($"Not enough gold.");
            }

            // Check if player has room in inventory
            if (player.InventorySlots.Count >= 16)
            {
                return Problem($"Inventory is full for player with Id = {playerId}");
            }

            // Check if item already exists in players inventory
            if (player.InventorySlots.Any(i => i.ItemId == itemId))
            {
                return Problem($"Player with Id = {playerId} already has item with Id = {itemId} in their inventory.");
            }

            // Subtract item price from player currency
            player.Currency = player.Currency - shopItem.Price;

            InventorySlot item = new InventorySlot()
            {
                PlayerId = playerId,
                ItemId = itemId
            };

            player.InventorySlots.Add(item);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("{playerId:int}/remove/{itemId:int}")]
        public async Task<IActionResult> RemovePlayerInventoryItem(int playerId, int itemId)
        {
            // Load player and player's inventory
            var inventory = await dbContext.Inventory.Where(i => i.PlayerId == playerId).ToListAsync();
            var items = await dbContext.Items.ToListAsync();
            // Check if inventory could be found and if the itemId exists in the Items table.
            if (inventory is null || !items.Any(i => i.Id == itemId))
            {
                return NotFound($"Inventory for player with Id = {playerId} could not be found or Item with Id = {itemId} does not exist.");
            }

            // Remove items with the requested ItemId
            foreach (var inventoryItem in inventory)
            {
                // Note: This way of removing all items needs to be changed if we define inventory SlotId or something that lets you have multiple of the same item.
                // As it is now this would remove all items with the given itemId.
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
