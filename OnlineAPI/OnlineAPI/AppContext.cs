using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;

namespace OnlineAPI
{
    public class AppContext : DbContext
    {
        public DbSet<Entities.Project> Projects { get; set; }
        public DbSet<Entities.Task> Tasks { get; set; }
        public DbSet<Entities.User> Users { get; set; }

        public DbSet<Entities.ProjectMember> ProjectMembers { get; set; }
        public DbSet<Entities.Invitation> Invitations { get; set; }

        public AppContext(DbContextOptions<AppContext> options)
           : base(options)
        {
        }

       

    }
}
