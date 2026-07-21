using Microsoft.EntityFrameworkCore;
using PasteService.Models;

namespace PasteService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Paste> Pastes { get; set; }
    }
}
