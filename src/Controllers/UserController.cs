using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {

        private readonly DBContext dbContext;
        public UserController(DBContext dbContext) {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if(rows > 30){
                return BadRequest("The number of rows cannot exceed 10");
            }
            var users = await dbContext.Users.AsNoTracking().Skip((page - 1) * rows).ToListAsync();

            if(users == null || users.Count == 0) {
                return NotFound("No users found");
            }

            var response = users.Select(u => new UserResponse(u.Username, u.Email, u.Role));
            return Ok(response);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            var response = new UserResponse(user.Username, user.Email, user.Role);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserRequest request) {

            if (ModelState.IsValid) {
                return BadRequest();
            }

            var user = new User(request.Username, request.Password.HashPassword(), request.Email, request.Role);

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var response = new UserResponse(user.Username, user.Email, user.Role);
            return Created($"User {user.Username} successfully created", user.Id);
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> Update([FromRoute] Guid Id, UserUpdate request) {

            if (ModelState.IsValid) {
                return BadRequest();
            }

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            user.Username = request.Username ?? user.Username;
            user.Email = request.Email ?? user.Email;
            user.Password = request.Password != null ? request.Password.HashPassword() : user.Username;
            user.Role = request.Role ?? user.Role;
            //user.UpdatedBy = 
            user.UpdatedOn = DateTime.UtcNow;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            var response = new UserResponse(user.Username, user.Email, user.Role);
            return Ok(response);
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
