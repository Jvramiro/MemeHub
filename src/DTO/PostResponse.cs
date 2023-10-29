namespace MemeHub.DTO {
    public record PostResponse(
        Guid id, string title, string imageUrl, string ownerUsername, int ratingLike, int ratingDislike,
        int commentCount, Guid createdBy, DateTime createdOn
    );
}
