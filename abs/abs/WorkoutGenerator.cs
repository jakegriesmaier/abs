using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace abs {
    //experience level, gender, workout length, workout times per week, goal


    public abstract class MuscleGroup {
        public int totalExercises;
        public int numSubgroup1Exercises;
        public int numSubgroup2Exercises;
        public int numSubgroup3Exercises;

        public MuscleGroup(string muscle) {
            this.totalExercises = 0;
            this.numSubgroup1Exercises = 0;
            this.numSubgroup2Exercises = 0;
            this.numSubgroup3Exercises = 0;
        }

        public void addExercise() {
            this.totalExercises++;
        }

        public void removeExercise() {
            this.totalExercises--;
        }

        public void addToSubgroup1() {
            this.numSubgroup1Exercises++;
        }

        public void removeFromSubgroup1() {
            this.numSubgroup1Exercises--;
            removeExercise();
        }

        public void addToSubgroup2() {
            this.numSubgroup2Exercises++;
        }

        public void removeFromSubgroup2() {
            this.numSubgroup2Exercises--;
            removeExercise();
        }

        public void addToSubgroup3() {
            this.numSubgroup3Exercises++;
        }

        public void removeFromSubgrou31() {
            this.numSubgroup3Exercises--;
            removeExercise();
        }

        public void calculatesubgroups() {
            int temp = totalExercises;
            while (temp != 0) {
                if (temp > 0) {
                    addToSubgroup1();
                    temp--;
                }
                if (temp > 0) {
                    addToSubgroup2();
                    temp--;
                }
                if (temp > 0) {
                    addToSubgroup3();
                    temp--;
                }
            }
        }

    }


    //TODO finish
    public class Plan {
        private static string[] equipmentAvailable = { "Bench", "Dumbbells" };//should be user input
        private static string[] equipmentAvailable2 = { "Bench", "Dumbbells" };//should be user input
        private static string[] weights = { "Bench", "Dumbbells" };//should be user input
        private int experienceLevel;
        private bool gender;//male = true;
        public int workoutLength;//30, 45, 60, 75, 90, 105, 120 (minutes)
        public int workoutsPerWeek;//2,3,4,5, or 6
        public int goal;//1=gain muscle, 2=lose fat, 3=maintain weight
        public int totalNumExercises;
        public int totalMinutes;
        public int numExercisesPerMainMuscleGroup;
        public int numExercisesPerSubgroup1;//number of exercises per subgroup per main bodypart for subgroup 1
        public int numExercisesPerSubgroup2;//number of exercises per subgroup per main bodypart for subgroup 2
        public int numExercisesPerSubgroup3;//number of exercises per subgroup per main bodypart for subgroup 3
        public MuscleGroup chest;
        public MuscleGroup back;
        public MuscleGroup legs;
        public MuscleGroup shoulders;
        public MuscleGroup arms;
        public MuscleGroup abs;

        UsersPossibleExerciseList exercises = new UsersPossibleExerciseList();
        List<Exercise> myExercises = new List<Exercise>();

        public Plan(int experienceLevel, bool gender, int workoutLength, int workoutsPerWeek, int goal) {
            //exercises.sortExercisesByEquipment(equipmentAvailable, equipmentAvailable2, weights);
            //myExercises = exercises.getExercises();
            this.experienceLevel = experienceLevel;
            this.gender = gender;
            this.workoutLength = workoutLength;
            this.workoutsPerWeek = workoutsPerWeek;
            this.goal = goal;
        }

        //selects the style of workout for the plan 
        public string selectStyle(int experienceLevel) {
            throw new NotImplementedException();
        }

        public void setNumExercisesPerMainGroup() {
            totalMinutes = workoutLength * workoutsPerWeek;
            totalNumExercises = totalMinutes / 15;
            int temp = totalNumExercises;
            while(temp != 0) {
                if(temp > 0) {
                    chest.addExercise();
                    temp--;
                }
                if (temp > 0) {
                    back.addExercise();
                    temp--;
                }
                if (temp > 0) {
                    legs.addExercise();
                    temp--;
                }
                if (temp > 0) {
                    shoulders.addExercise();
                    temp--;
                }
                if (temp > 0) {
                    arms.addExercise();
                    temp--;
                }
                if (temp > 0) {
                    abs.addExercise();
                    temp--;
                }
            }
        }


        public void calculateNumExercisesPerSubgroup() {
            chest.calculatesubgroups();
            back.calculatesubgroups();
            legs.calculatesubgroups();
            shoulders.calculatesubgroups();
            arms.calculatesubgroups();
            abs.calculatesubgroups();
        }
            

        //selects what days bodyparts will be worked out
        public string selectSplit(int workoutsPerWeek) {
            throw new NotImplementedException();
        }




    }


    //TODO create a list of possible exercises based on what equipment the user has avaiable 
    public class UsersPossibleExerciseList {

        static List<Exercise> Exercises = new List<Exercise>();
        private string exerciseName;
        private int numExercises;


        public UsersPossibleExerciseList() {
            this.numExercises = 0;
        }

        public string exercisesAsHTML() {
            throw new NotImplementedException();
        }

        public void sortExercisesByEquipment(string[] equipmentAvailable, string[] equipmentAvailable2, string[] weights) {
            throw new NotImplementedException();
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
