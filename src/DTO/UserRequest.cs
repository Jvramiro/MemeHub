using MemeHub.Models;

namespace MemeHub.DTO {
    public record UserRequest(string Username, string Email, string Password, Role Role);
}
