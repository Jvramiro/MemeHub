namespace MemeHub.Models {
    public class Post : Entity {
        public Guid Owner { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }

        public Post(Guid Owner, string Title, string ImageUrl, Guid? CreatedBy = null) {
            this.Owner = Owner;
            this.Title = Title;
            this.ImageUrl = ImageUrl;
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
