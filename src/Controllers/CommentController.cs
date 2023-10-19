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
    public class CommentController : ControllerBase {

        private readonly DBContext dbContext;
        public CommentController(DBContext dbContext) {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if (rows > 30) {
                return BadRequest("The number of rows cannot exceed 50");
            }

            var comments = await dbContext.Comments.AsNoTracking().Skip((page - 1) * rows).ToListAsync();

            if (comments == null || comments.Count == 0) {
                return NotFound("No Comments found");
            }

            return Ok(comments);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var comment = await dbContext.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id);

            if (comment == null) {
                return NotFound("Comment not found");
            }

            return Ok(comment.Text);
        }

        [HttpPost]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Create([FromBody] CommentRequest request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var comment = new Comment(request.Text, request.PostId, Guid.Empty);

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

            comment.Text = Text;

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

            dbContext.Comments.Remove(comment);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
