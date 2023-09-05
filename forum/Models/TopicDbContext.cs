using System;
using Microsoft.EntityFrameworkCore;

namespace forum.Models;

public class TopicDbContext : DbContext
{
    public TopicDbContext(DbContextOptions<TopicDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Topic> Topics { get; set; }
}