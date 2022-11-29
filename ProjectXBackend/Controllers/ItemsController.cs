using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var items = await dbContext.Items
                .Select(i => new
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = i.Type,                    
                    ItemStats = i.ItemStats.Select(x => new
                    {
                        StatsId = x.StatsId,
                        StatName = x.Stats.StatName,
                        StatsValue = x.StatsValue
                    })
                }).ToListAsync();

            return Ok(items);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await dbContext.Items
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = i.Type,
                    ItemStats = i.ItemStats.Select(x => new
                    {
                        StatName = x.Stats.StatName,
                        StatsValue = x.StatsValue,
                    })
                }).FirstOrDefaultAsync();

            if (item is null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}
