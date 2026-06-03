using Microsoft.EntityFrameworkCore;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class WorkoutContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ProgramExercise> ProgramExercises { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

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
            // Отключаем каскадное удаление для всех FOREIGN KEY
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Настройка ProgramExercise
            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.Program)
                .WithMany()
                .HasForeignKey(pe => pe.ProgramId);

            modelBuilder.Entity<ProgramExercise>()
                .HasOne(pe => pe.Exercise)
                .WithMany()
                .HasForeignKey(pe => pe.ExerciseId);

            // Настройка Workout
            modelBuilder.Entity<Workout>()
                .HasOne(w => w.Program)
                .WithMany()
                .HasForeignKey(w => w.ProgramId);

            // Настройка WorkoutExercise
            modelBuilder.Entity<WorkoutExercise>()
                .HasOne(we => we.Workout)
                .WithMany(w => w.WorkoutExercises)
                .HasForeignKey(we => we.WorkoutId);

            modelBuilder.Entity<WorkoutExercise>()
                .HasOne(we => we.Exercise)
                .WithMany()
                .HasForeignKey(we => we.ExerciseId);

            // Индексы
            modelBuilder.Entity<Activity>().HasIndex(a => a.Date);
            modelBuilder.Entity<Activity>().HasIndex(a => a.ClientId);
            modelBuilder.Entity<ProgramExercise>().HasIndex(pe => pe.ProgramId);
            modelBuilder.Entity<ProgramExercise>().HasIndex(pe => pe.ExerciseId);
            modelBuilder.Entity<Workout>().HasIndex(w => w.Date);
            modelBuilder.Entity<WorkoutExercise>().HasIndex(we => we.WorkoutId);
        }
    }
}