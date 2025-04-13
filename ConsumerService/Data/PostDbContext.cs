using ConsumerService.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsumerService.Data
{
    public class PostDbContext : DbContext
    {
        public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }
    }
}
