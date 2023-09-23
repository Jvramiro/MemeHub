using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MemeHub.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase {

        private readonly DBContext dbContext;
        public PostController(DBContext dbContext) {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 30");
            }
            var posts = await dbContext.Posts.AsNoTracking().Skip((page - 1) * rows).ToListAsync();

            if (posts == null || posts.Count == 0) {
                return NotFound("No posts found");
            }

            var response = new List<PostResponse>();

            foreach(var post in posts) {
                int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
                int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();

                response.Add(new PostResponse(post.Title, post.ImageUrl, ratingTrue, ratingFalse));
            }

            return Ok(response);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var post = await dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Id);

            if (post == null) {
                return NotFound("Post not found");
            }

            int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
            int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();

            var response = new PostResponse(post.Title, post.ImageUrl, ratingTrue, ratingFalse);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PostRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var post = new Post(Guid.Empty, request.title, request.ImageUrl);

            await dbContext.Posts.AddAsync(post);
            await dbContext.SaveChangesAsync();

            var response = new PostResponse(post.Title, post.ImageUrl, 0, 0);
            return Created($"Post successfully created", post.Id);
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var post = await dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Id);

            if (post == null) {
                return NotFound("Post not found");
            }

            dbContext.Posts.Remove(post);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
