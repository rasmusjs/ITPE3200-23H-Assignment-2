using forum.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace forum.DAL;

// The ORM (Object-Relational Mapper) for the application
// This is the communication between the application and the database, defining data structures and relationships.
public class ForumDbContext : IdentityDbContext
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    // Getter and setters for the models
    //public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }

    //public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Comment> Comments { get; set; }

    // Configuring the relationships and schemas for the entities in the database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuring the many to many relationship between tags and posts
        // Source: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
        // Link Posts and Tags using the help table PostTag
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTag"));

        // Configuring the one-to-many relationship between Posts and Categories
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);

        // Configuring the self-referencing relationship for Comments (For replies to comments)
        modelBuilder.Entity<Comment>()
            .HasMany(c => c.CommentReplies)
            .WithOne(c => c.ParentComment)
            .HasForeignKey(c => c.ParentCommentId);
    }

    // Enable Lazy Loading for loading data when it is needed
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}