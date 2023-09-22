using MemeHub.Models;

namespace MemeHub.DTO {
    public record UserResponse(string Username, string Email, Role Role);
}
