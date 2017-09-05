using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    //TODO finish
    public class Plan {
        private static string[] equipmentAvailable = { "Bench", "Dumbbells" };
        //ExerciseList exercises = new ExerciseList(1, equipmentAvailable);
        ExerciseList exercises = new ExerciseList();
        List<Exercise> myExercises = new List<Exercise>();

        public Plan() {
            exercises.sortExercisesByEquipment(equipmentAvailable);
            myExercises = exercises.getExercises();
        }


    }

    public class ExerciseList {

        static List<Exercise> Exercises = new List<Exercise>();
        private int experienceLevel;
        private string exerciseName;
        private int[] reps;
        private int sets;
        private string style;
        private int[] weights;
        private string equipment;
        private int numExercises;

        //TODO work on this
        public ExerciseList() {
            this.numExercises = 0;
        }

        public void sortExercisesByEquipment(string[] equipmentAvailable) {
            bool keep = false;
            foreach (Exercise e in Exercises) {
                for (int i = 0; i < equipmentAvailable.Length; i++) {
                    if (e.getEquipment() == equipmentAvailable[i]) {
                        keep = true;
                    }
                }
                if (keep == false) removeExercise(e);
                keep = false;
            }
        }

        public void addExercise(Exercise exercise) {
            Exercises.Add(exercise);
            numExercises++;
        }

        public void removeExercise(Exercise exercise) {
            Exercises.Remove(exercise);
            numExercises--;
        }

        public List<Exercise> getExercises() {
            return Exercises;
        }
    }

}
