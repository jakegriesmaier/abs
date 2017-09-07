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


        public ExerciseList() {
            this.numExercises = 0;
        }

        //TODO find more elegant way to do this
        public string generateAllExerciseInformationHTMLString() {
            string exerciseInformation = "<table><tr><th>Exercise</th><th>Specified Area</th><th>Reps</th><th>Sets</th><th>Weights</th><th>Equipment Required</th></tr><tr>";
            foreach(Exercise e in Exercises) {
                exerciseInformation += "<td>" + e.exerciseName + "</td>";
            }
            exerciseInformation += "</tr><tr>";
            foreach (Exercise e in Exercises) {
                exerciseInformation += "<td>" + e.specifiedArea + "</td>";
            }
            exerciseInformation += "</tr><tr>";
            foreach (Exercise e in Exercises) {
                exerciseInformation += "<td>" + string.Join(",", e.reps) + "</td>";
            }
            exerciseInformation += "</tr><tr>";
            foreach (Exercise e in Exercises) {
                exerciseInformation += "<td>" + e.sets.ToString() + "</td>";
            }
            exerciseInformation += "</tr><tr>";
            foreach (Exercise e in Exercises) {
                exerciseInformation += "<td>" + string.Join(",", e.sets) + "</td>";
            }
            exerciseInformation += "</tr><tr>";
            foreach (Exercise e in Exercises) {
                exerciseInformation += "<td>" + e.equipment + "</td>";
            }
            return exerciseInformation += "</tr></table>"; ;
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
