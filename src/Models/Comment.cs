namespace MemeHub.Models {
    public class Comment : Entity{

        public string Text { get; set; }
        public Guid PostId { get; set; }
        public Guid? TaggedId { get; set; }

        public Comment(string Text, Guid PostId, Guid? CreatedBy = null, Guid? TaggedId = null) {
            this.Text = Text;
            this.PostId = PostId;
            this.TaggedId = TaggedId;
            this.IsActive = true;

            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            if (CreatedBy != null) {
                this.CreatedBy = (Guid)CreatedBy;
                this.UpdatedBy = (Guid)CreatedBy;
            }
        }

    }
}
