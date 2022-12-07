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
            var stats = await dbContext.Stats.ToListAsync();

            List<StatDTO> statList = new List<StatDTO>();
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
