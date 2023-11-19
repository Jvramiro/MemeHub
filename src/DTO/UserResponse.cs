using MemeHub.Models;

namespace MemeHub.DTO {
    public record UserResponse(Guid Id, string Username, string Email, DateTime Birthday, Role Role);
}
