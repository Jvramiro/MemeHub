namespace MemeHub.Models {
    public enum Role { User, Adm }
    public class User : Entity {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public DateTime Birthday { get; set; }

        public User(string Username, string Password, string Email, Role Role, DateTime Birthday) {
            this.Username = Username;
            this.Password = Password;
            this.Email = Email;
            this.Role = Role;
            this.Birthday = Birthday;
            this.IsActive = true;

            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;

        }


    }
}
