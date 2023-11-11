using MemeHub.Data;
using MemeHub.DTO;
using MemeHub.Extensions;
using MemeHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeHub.Controllers {
    [Route("api/login")]
    [ApiController]
    public class TokenController : ControllerBase {

        private readonly DBContext dbContext;
        private readonly IConfiguration configuration;
        public TokenController(DBContext dbContext, IConfiguration configuration) {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request) {

            var user = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password.HashPassword());

            if (user == null) {
                return Unauthorized();
            }

            var token = TokenService.GenerateToken(user.Email, user.Id, user.Role, configuration);

            var response = new {
                Token = token,
                ID = user.Id
            };

            return Ok(response);

        }

    }
}
