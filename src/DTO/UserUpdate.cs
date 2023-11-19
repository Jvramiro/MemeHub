using MemeHub.Models;

namespace MemeHub.DTO {
    public record UserUpdate(string? Username, string? Email, string? Password, DateTime? Birthday, Role? Role);
}
