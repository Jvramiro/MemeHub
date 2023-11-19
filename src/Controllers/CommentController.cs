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
    public class CommentController : ControllerBase {

        private readonly DBContext dbContext;
        public CommentController(DBContext dbContext) {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 50");
            }

            var comments = await dbContext.Comments.AsNoTracking().Skip((page - 1) * rows)
                                                    .OrderByDescending(c => c.CreatedOn)
                                                    .ToListAsync();

            if (comments == null || comments.Count == 0) {
                return NotFound("No Comments found");
            }

            return Ok(comments);
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var comment = await dbContext.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);

            if (comment == null) {
                return NotFound("Comment not found");
            }

            return Ok(comment.Text);
        }

        [HttpGet("user/{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUser([FromRoute] Guid Id, int page = 1, int rows = 10, int limit = 200) {

            var getComments = await dbContext.Comments.AsNoTracking().Where(c => c.CreatedBy == Id)
                                .OrderByDescending(c => c.CreatedOn).Take(limit).ToListAsync();

            var comments = getComments.Skip((page - 1) * rows);

            if (comments == null || comments.Count() == 0) {
                return NotFound("No Comments found");
            }

            return Ok(comments);
        }

        [HttpPost]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Create([FromBody] CommentRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var post = await dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PostId);
            if (post == null) {
                return NotFound("Post not found");
            }

            Guid userId;
            if(!Guid.TryParse(HttpContext.User.FindFirst("Id").Value, out userId)){
                return BadRequest("There's no valid Id on Token");
            }

            var comment = new Comment(request.Text, userId, request.PostId, Guid.Empty);

            if(request.TaggedId != null) {
                comment.TaggedId = (Guid)request.TaggedId;
            }

            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();

            return Created($"Comment successfully created", comment.Id);
        }

        [HttpPut("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Update([FromRoute] Guid Id, [FromBody] string Text) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var comment = await dbContext.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);

            if (comment == null) {
                return NotFound("Comment not found");
            }

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (comment.Owner.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            comment.Text = Text;

            Guid updatedBy = Guid.Empty;
            if (Guid.TryParse(userId, out updatedBy)) {
                comment.UpdatedBy = updatedBy;
            }

            dbContext.Comments.Update(comment);
            await dbContext.SaveChangesAsync();

            return Ok($"Comment sucessfully updated");
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var comment = await dbContext.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);

            if (comment == null) {
                return NotFound("Comment not found");
            }

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (comment.Owner.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            dbContext.Comments.Remove(comment);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
