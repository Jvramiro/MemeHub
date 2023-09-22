using MemeHub.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MemeHub.Data {
    public class DBContext : DbContext {

        public DBContext(DbContextOptions<DBContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<User>()
                .Property(p => p.Username).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>()
                .HasIndex(e => e.Username).IsUnique();
            modelBuilder.Entity<User>()
                .Property(p => p.Email).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>()
                .HasIndex(e => e.Email).IsUnique();
            modelBuilder.Entity<User>()
                .Property(p => p.Password).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>()
                .HasIndex(e => e.Password).IsUnique();
            modelBuilder.Entity<User>()
                .Property(p => p.Role).IsRequired();

            modelBuilder.Entity<Post>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Post>()
                .Property(p => p.Owner).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.Title).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.ImageUrl).IsRequired();

            modelBuilder.Entity<Comment>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Comment>()
                .Property(p => p.Text).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(p => p.PostId).IsRequired();

            modelBuilder.Entity<Rating>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Rating>()
                .Property(p => p.Owner).IsRequired();
            modelBuilder.Entity<Rating>()
                .Property(p => p.PostId).IsRequired();
            modelBuilder.Entity<Rating>()
                .Property(p => p.Value).IsRequired();

        }

    }
}
