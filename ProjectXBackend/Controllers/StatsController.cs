using ProjectXBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectXBackend.DTOs;

namespace ProjectXBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : Controller
    {
        private ApiDbContext dbContext;
        public StatsController(ApiDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            // Load all stats
            var stats = await dbContext.Stats.ToListAsync();

            // Create a list of StatDTO
            List<StatDTO> statList = new List<StatDTO>();
            // Loop through all stats from the Database and map the properties to the DTO
            foreach (var stat in stats)
            {
                StatDTO dto = new StatDTO();
                dto.Id = stat.Id;
                dto.Name = stat.StatName;

                statList.Add(dto);
            }
            return Ok(statList);
        }
    }
}
