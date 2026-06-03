using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.WorkoutTracker;

namespace WorkoutTracker
{
    public class WorkoutExercise
    {
        [Key]
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public int ExerciseId { get; set; }
        public int Sets { get; set; }      // выполнено подходов
        public int Reps { get; set; }      // повторений
        public int Weight { get; set; }    // вес
        public int Duration { get; set; }  // время в минутах
        public string Notes { get; set; } = string.Empty;

        public virtual Workout? Workout { get; set; }
        public virtual Exercise? Exercise { get; set; }
    }
}
