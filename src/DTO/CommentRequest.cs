namespace MemeHub.DTO {
    public record CommentRequest(string Text, Guid PostId, Guid? TaggedId = null);
}
