using MemeHub.Models;

namespace MemeHub.DTO {
    public record UserRequest(string Username, string Email, string Password, DateTime Birthday, Role Role = 0);
}
