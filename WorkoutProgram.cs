using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class WorkoutProgram
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }
        public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();
        public virtual ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    }
}
