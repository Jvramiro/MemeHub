using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MemeHub.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase {

        private readonly DBContext dbContext;
        public RatingController(DBContext dbContext) {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if (rows > 50) {
                return BadRequest("The number of rows cannot exceed 50");
            }

            var rating = await dbContext.Rating.AsNoTracking().Skip((page - 1) * rows).ToListAsync();

            if (rating == null || rating.Count == 0) {
                return NotFound("No Rating found");
            }

            return Ok(rating);
        }

        [HttpPost]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Create([FromBody] RatingRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var rating = new Rating(Guid.Empty, request.PostId, request.Value);

            await dbContext.Rating.AddAsync(rating);
            await dbContext.SaveChangesAsync();

            return Created($"Rating successfully created", rating.Id);
        }

        [HttpPut("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Update([FromRoute] Guid Id, [FromBody] RatingUpdate request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var rating = await dbContext.Rating.AsNoTracking().FirstOrDefaultAsync(r => r.Id == Id);

            if (rating == null) {
                return NotFound("Rating not found");
            }

            rating.IsActive = request.IsActive;
            rating.Value = !request.IsActive ? false : request.Value;

            dbContext.Rating.Update(rating);
            await dbContext.SaveChangesAsync();

            return Ok($"Rating sucessfully updated to {request.Value}");
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var rating = await dbContext.Rating.AsNoTracking().FirstOrDefaultAsync(r => r.Id == Id);

            if (rating == null) {
                return NotFound("Post not found");
            }

            dbContext.Rating.Remove(rating);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
