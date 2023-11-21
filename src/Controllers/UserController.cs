using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Models;
using MemeHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        [Authorize(Roles = "Adm")]
        public async Task<IActionResult> Get(int page = 1, int rows = 10) {

            if(rows > 30){
                return BadRequest("The number of rows cannot exceed 30");
            }
            var users = await dbContext.Users.AsNoTracking().Skip((page - 1) * rows).ToListAsync();

            if(users == null || users.Count == 0) {
                return NotFound("No users found");
            }

            var response = users.Select(u => new UserResponse(u.Id, u.Username, u.Email, u.Birthday, u.Role));
            return Ok(response);
        }

        [HttpGet("{Id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid Id) {

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            var response = new UserResponse(user.Id, user.Username, user.Email, user.Birthday, user.Role);
            return Ok(response);
        }

        [HttpGet("current")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> GetCurrentUser() {

            var userId = HttpContext.User.FindFirst("Id").Value;
            Guid UserGuid = Guid.Empty;

            if (userId == null || !Guid.TryParse(userId, out UserGuid)) {
                return BadRequest("There's no valid Id on Token");
            }

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == UserGuid);

            if (user == null) {
                return NotFound("User not found");
            }

            var response = new UserResponse(user.Id, user.Username, user.Email, user.Birthday, user.Role);
            return Ok(response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserRequest request) {

            if (!ModelState.IsValid){
                return BadRequest();
            }

            if (!PasswordService.CheckStrength(request.Password)) {
                return BadRequest("Weak password");
            }

            if (request.Role == Role.Adm) {
                if (HttpContext != null && HttpContext.User != null) {
                    var userId = HttpContext.User.FindFirst("Id")?.Value;
                    if (userId == null || !HttpContext.User.IsInRole("Adm")) {
                        return Unauthorized("User not authorized to create an Adm User");
                    }
                }
                else {
                    return Unauthorized("User not authorized to create an Adm User");
                }
            }

            var user = new User(request.Username, request.Password.HashPassword(),
                                request.Email, request.Role, request.Birthday);

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            var response = new UserResponse(user.Id, user.Username, user.Email, user.Birthday, user.Role);
            return Created($"User {user.Username} successfully created", user.Id);
        }

        [HttpPut("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Update([FromRoute] Guid Id, UserUpdate request) {

            if (!ModelState.IsValid) {
                return BadRequest();
            }

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (user.Id.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            user.Username = request.Username ?? user.Username;
            user.Email = request.Email ?? user.Email;
            user.Password = request.Password != null ? request.Password.HashPassword() : user.Password;
            user.Role = request.Role ?? user.Role;
            user.Birthday = request.Birthday ?? user.Birthday;
            user.UpdatedOn = DateTime.UtcNow;

            Guid updatedBy = Guid.Empty;
            if (Guid.TryParse(userId, out updatedBy)) {
                user.UpdatedBy = updatedBy;
            }

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            var response = new UserResponse(user.Id, user.Username, user.Email, user.Birthday, user.Role);
            return Ok(response);
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = "User, Adm")]
        public async Task<IActionResult> Delete([FromRoute] Guid Id) {

            var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);

            if (user == null) {
                return NotFound("User not found");
            }

            var userId = HttpContext.User.FindFirst("Id").Value;
            if (userId == null) {
                return BadRequest("There's no valid Id on Token");
            }
            if (user.Id.ToString() != userId && !HttpContext.User.IsInRole("Adm")) {
                return Unauthorized("User not authorized to make changes in this slot");
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Ok($"Id {Id} successfully deleted");
        }


    }
}
