using forum.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace forum.DAL;

// The ORM (Object-Relational Mapper) for the application
// This is the communication between the application and the database, defining data structures and relationships.
public class ForumDbContext : IdentityDbContext<ApplicationUser>
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    // Getter and setters for the models
    // public DbSet<User> CustomUsers { get; set; }

    public DbSet<ApplicationUser> CustomUsers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }

    //public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Comment> Comments { get; set; }

    // Configuring the relationships and schemas for the entities in the database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuring the many to many relationship between tags and posts
        // Source: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
        // Link Posts and Tags using the help table PostTag
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTag"));

        // Configuring the one-to-many relationship between Posts and Categories
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);

        // Configuring the one-to-many relationship between User and Posts
        modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior
                .SetNull); // If the user is deleted, the posts will not be deleted. User will show up a anonymous


        // Configuring the self-referencing relationship for Comments (For replies to comments)
        modelBuilder.Entity<Comment>()
            .HasMany(c => c.CommentReplies)
            .WithOne(c => c.ParentComment)
            .HasForeignKey(c => c.ParentCommentId);

        // Configuring the one-to-many relationship between User and Comments
        modelBuilder.Entity<Comment>().HasOne(p => p.User).WithMany(u => u.Comments).HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior
                .SetNull); // If the user is deleted, the posts will not be deleted. User will show up a anonymous


        //Fixes
        //Unhandled exception. System.InvalidOperationException: The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'. For more information on keyless entity types, see https://go.microsoft.com/fwlink/?linkid=2141943.
        //Source: https://stackoverflow.com/questions/39576176/is-base-onmodelcreatingmodelbuilder-necessary
        base.OnModelCreating(modelBuilder);
    }


    // Enable Lazy Loading for loading data when it is needed
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}