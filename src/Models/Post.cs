namespace MemeHub.Models {
    public class Post : Entity {
        public Guid Owner { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }

        public Post(Guid Owner, string Title, string ImageUrl) {
            this.Owner = Owner;
            this.Title = Title;
            this.ImageUrl = ImageUrl;
            this.IsActive = true;

            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            this.CreatedBy = Owner;
            this.UpdatedBy = Owner;
        }

    }
}
