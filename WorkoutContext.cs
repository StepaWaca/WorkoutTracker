using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class WorkoutContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = DatabaseConfig.LoadFromFile();
                optionsBuilder.UseSqlServer(config.GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Exercise)
                .WithMany()
                .HasForeignKey(a => a.ExerciseId);
        }
    }
}