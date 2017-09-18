using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace abs {
    //experience level, gender, workout length, workout times per week, goal



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

        //determines the number of exercises that the plan will contain (based on the workout length)
        public int numExercises() {
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
