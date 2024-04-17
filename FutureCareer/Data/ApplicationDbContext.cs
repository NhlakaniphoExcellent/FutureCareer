using FutureCareer.Models;
using Microsoft.EntityFrameworkCore;

namespace FutureCareer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
           
        }
        public DbSet<Student> Students { get; set; }
    }
}
