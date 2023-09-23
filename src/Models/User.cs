namespace MemeHub.Models {
    public enum Role { User, Adm }
    public class User : Entity {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }

        public User(string Username, string Password, string Email, Role role, Guid CreatedBy) {
            this.Username = Username;
            this.Password = Password;
            this.Email = Email;
            this.Role = role;
            this.IsActive = true;

            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

            this.CreatedBy = (Guid)CreatedBy;
            this.UpdatedBy = (Guid)CreatedBy;

        }


    }
}
