using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class Workout
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalDuration { get; set; } // общая длительность в минутах
        public string Notes { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public int? ProgramId { get; set; }

        public virtual Client? Client { get; set; }
        public virtual WorkoutProgram? Program { get; set; }
        public virtual ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
    }
}
