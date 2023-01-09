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
            // Load all items and Include their ItemStats and then include the ItemStats' Stats values
            var data = await dbContext.Items.Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).ToListAsync();

            // Loop through all loaded items and map the properties to the DTO
            // Using a DTO lets us return the data in a format we choose even if it's different from how the data is set up in the database.
            foreach (var item in data)
            {
                ItemDTO dto = new ItemDTO();
                dto.ItemId = item.Id;
                dto.Name = item.Name;
                dto.Type = item.Type;
                dto.Price = item.Price;
                dto.Image = item.Image;
                // ItemStats is a collection, loop through all ItemStats and map the properties to the DTO
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
            // Load the item from the id parameter, Include ItemStats and ThenInclude the ItemStats' Stats values.
            var item = await dbContext.Items.Where(x => x.Id == id).Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).FirstOrDefaultAsync();

            // Check if item exists in the Database
            if (item is null)
            {
                return NotFound($"Item with the ID = {id} could not be found.");
            }

            // Create a new ItemDTO and map the properties
            ItemDTO dto = new ItemDTO();
            dto.ItemId = item.Id;
            dto.Name = item.Name;
            dto.Type = item.Type;
            dto.Price = item.Price;
            dto.Image = item.Image;
            // ItemStats is a collection, loop through all ItemStats and map the properties to the DTO
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
        [Route("stats/{id:int}")]
        public async Task<IActionResult> GetItemStats(int id)
        {
            // This method currently loads an item instead of just ItemStats, it could be simplified by doing it like the line below. I didn't think of it when I wrote it. My bad <|,'-[} )
            //var itemStats = await dbContext.ItemStats.Where(i => i.ItemId == id).Include(stat => stat.Stats).ToListAsync();

            var item = await dbContext.Items.Where(x => x.Id == id).Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).FirstOrDefaultAsync();

            if (item is null)
            {
                return NotFound($"Item with the ID = {id} could not be found.");
            }

            List<ItemStatDTO> itemStatDTOs = new List<ItemStatDTO>();
            // Loop through all ItemStats. Creates a new ItemStatDTO, maps the properties and then adds to the list before returning the list.
            foreach (var itemStat in item.ItemStats)
            {
                ItemStatDTO dto = new ItemStatDTO();
                dto.StatsId = itemStat.StatsId;
                dto.StatName = itemStat.Stats.StatName;
                dto.StatsValue = itemStat.StatsValue;

                itemStatDTOs.Add(dto);
            }
            return Ok(itemStatDTOs);
        }
    }
}
