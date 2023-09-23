namespace MemeHub.Models {
    public class Rating{
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public Guid PostId { get; set; }
        public bool Value { get; set; }
        public bool IsActive { get; set; }

        public Rating(Guid Owner, Guid PostId, bool Value) {
            this.Owner = Owner;
            this.PostId = PostId;
            this.Value = Value;
        }
    }
}
