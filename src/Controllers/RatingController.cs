using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

            Guid userId = Guid.Empty;
            if (!Guid.TryParse(HttpContext.User.FindFirst("Id").Value, out userId)) {
                return BadRequest("There's no valid Id on Token");
            }

            var rating = new Rating(userId, request.PostId, request.Value);

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

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (rating.Owner.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
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

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (rating.Owner.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            dbContext.Rating.Remove(rating);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
