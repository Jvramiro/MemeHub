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
        [AllowAnonymous]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 30");
            }
            var posts = await dbContext.Posts.AsNoTracking()
                                                .OrderByDescending(p => p.CreatedOn).Skip((page - 1) * rows)
                                                .ToListAsync();

            if (posts == null || posts.Count == 0) {
                return NotFound("No posts found");
            }

            var response = new List<PostResponse>();

            foreach(var post in posts) {
                int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
                int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();
                int commentCount = await dbContext.Comments.CountAsync(c => c.PostId == post.Id);

                response.Add(new PostResponse(post.Id, post.Title, post.ImageUrl, post.OwnerUsername, ratingTrue, ratingFalse,
                                                commentCount, post.Owner, post.CreatedOn));
                Console.WriteLine($"User: {post.OwnerUsername}");
            }

            return Ok(response);
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var post = await dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);

            if (post == null) {
                return NotFound("Post not found");
            }

            int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
            int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();
            int commentCount = await dbContext.Comments.CountAsync(c => c.PostId == post.Id);

            var response = new PostResponse(post.Id, post.Title, post.ImageUrl, post.OwnerUsername, ratingTrue, ratingFalse,
                                                commentCount, post.Owner, post.CreatedOn);

            return Ok(response);
        }

        [HttpGet("ranked")]
        [AllowAnonymous]
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
                int commentCount = await dbContext.Comments.CountAsync(c => c.PostId == post.Id);

                response.Add(new PostResponse(post.Id, post.Title, post.ImageUrl, post.OwnerUsername, ratingTrue, ratingFalse,
                                                commentCount, post.Owner, post.CreatedOn));
            }

            return Ok(response);
        }

        [HttpGet("user/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUser([FromRoute] Guid Id, int page = 1, int rows = 10, int limit = 200) {

            if (limit > 200) {
                return BadRequest("The limit of post to check cannot exceed 200");
            }
            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 30");
            }

            var getPosts = await dbContext.Posts.AsNoTracking().Where(p => p.CreatedBy == Id)
                            .OrderByDescending(p => p.CreatedOn).Take(limit).ToListAsync();

            var posts = getPosts.Skip((page - 1) * rows);

            if (posts == null) {
                return NotFound("No posts found");
            }

            var response = new List<PostResponse>();

            foreach (var post in posts) {
                int ratingTrue = await dbContext.Rating.Where(r => r.PostId == post.Id && r.Value && r.IsActive).CountAsync();
                int ratingFalse = await dbContext.Rating.Where(r => r.PostId == post.Id && !r.Value && r.IsActive).CountAsync();
                int commentCount = await dbContext.Comments.CountAsync(c => c.PostId == post.Id);

                response.Add(new PostResponse(post.Id, post.Title, post.ImageUrl, post.OwnerUsername, ratingTrue, ratingFalse,
                                                commentCount, post.Owner, post.CreatedOn));
            }

            return Ok(response);

        }

        [HttpPost]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Create([FromBody] PostRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            Guid userId = Guid.Empty;
            if (!Guid.TryParse(HttpContext.User.FindFirst("Id").Value, out userId)) {
                return BadRequest("There's no valid Id on Token");
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user == null) {
                return StatusCode(500, "There's no valid Id on Token");
            }

            var post = new Post(userId, request.title, request.ImageUrl, user.Username);

            await dbContext.Posts.AddAsync(post);
            await dbContext.SaveChangesAsync();

            var response = new PostResponse(post.Id, post.Title, post.ImageUrl, post.OwnerUsername, 0, 0, 0,
                                            post.Owner, post.CreatedOn);
            return Created($"Post successfully created", post.Id);
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var post = await dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Id);

            if (post == null) {
                return NotFound("Post not found");
            }

            var userId = HttpContext.User.FindFirst("Id").Value;
            if(userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (post.Owner.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            dbContext.Posts.Remove(post);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
