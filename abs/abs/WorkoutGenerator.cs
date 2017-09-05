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

        //make this so that it sorts the list for...
        public void sortExercises(int[] value) {
            Boolean keep = false;
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

    //change the getters and setters so that they are accessing the PostgreSQL database
    public class Exercise {

        private string exerciseName;
        private string mainBodyPart;
        private string specifiedArea;
        private string liftType;
        private int areaNumber;
        private bool requiresWeight;
        private int[] reps;
        private int sets;
        private string style;
        private int[] weights;
        private string equipment;


        public Exercise() { }

        public Exercise(string mainBodyPart, string specifiedArea, string liftType, int areaNumber, bool requiresWeight, string exerciseName, int[] reps, int sets, string style, int[] weights, string equipmemnt) {
            this.exerciseName = exerciseName;
            this.mainBodyPart = mainBodyPart;
            this.liftType = liftType;
            this.areaNumber = areaNumber;
            this.specifiedArea = specifiedArea;
            this.requiresWeight = requiresWeight;
            this.reps = reps;
            this.sets = sets;
            this.style = style;
            this.weights = weights;
            this.equipment = equipmemnt;
        }

        public string getMainBodyPart() {
            return this.mainBodyPart;
        }

        public void setMainBodyPart(string mainBodyPart) {
            this.mainBodyPart = mainBodyPart;
        }

        public string getspecifiedArea() {
            return this.specifiedArea;
        }

        //TODO ADD MORE SETTERS AND GETTERS
        public void setSpecifiedArea(string specifiedArea) {
            this.specifiedArea = specifiedArea;
        }

        public string getExerciseName() {
            return this.exerciseName;
        }

        public void setExerciseName(string exerciseName) {
            this.exerciseName = exerciseName;
        }

        public int getSets() {
            return this.sets;
        }

        public void setSets(int sets) {
            this.sets = sets;
        }

        public int[] getReps() {
            return this.reps;
        }

        public void setReps(int[] reps) {
            this.reps = reps;
        }

        public int[] getWeights() {
            return this.weights;
        }

        public void setWeights(int[] weights) {
            this.weights = weights;
        }

        public string getStyle() {
            return this.style;
        }

        public void setStyle(string style) {
            this.style = style;
        }

        public void setEquipment(string equipment) {
            this.equipment = equipment;
        }

        public string getEquipment() {
            return this.equipment;
        }
    }

}
