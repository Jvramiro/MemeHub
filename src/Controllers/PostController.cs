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

        [HttpGet("ranked")]
        public async Task<IActionResult> GetByRankingTrue(int page = 1, int rows = 10, int limit = 200) {

            if(limit > 200) {
                return BadRequest("The limit of post to check cannot exceed 200");
            }
            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 30");
            }

            var allPosts = await dbContext.Posts.AsNoTracking().OrderByDescending(
                p => dbContext.Rating.Count(r => r.PostId == p.Id && r.Value && r.IsActive))
                .Take(limit).ToListAsync();

            var posts = allPosts.Skip((page - 1) * rows);

            if (posts == null) {
                return NotFound("No posts found");
            }

            var response = new List<PostResponse>();
            foreach (var post in posts) {
                int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
                int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();

                response.Add(new PostResponse(post.Title, post.ImageUrl, ratingTrue, ratingFalse));
            }

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "User, Adm")]
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
        [Authorize(Roles = "User, Adm")]
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
