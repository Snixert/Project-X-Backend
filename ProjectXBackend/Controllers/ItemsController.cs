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
        [Route("stats/{id:int}")]
        public async Task<IActionResult> GetItemStats(int id)
        {
            var item = await dbContext.Items.Where(x => x.Id == id).Include(i => i.ItemStats).ThenInclude(stat => stat.Stats).FirstOrDefaultAsync();

            if (item is null)
            {
                return NotFound($"Item with the ID = {id} could not be found.");
            }

            List<ItemStatDTO> itemStatDTOs = new List<ItemStatDTO>();
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
