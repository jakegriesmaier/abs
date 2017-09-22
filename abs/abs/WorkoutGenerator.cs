using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monopage;

namespace abs {
    //experience level, gender, workout length, workout times per week, goal

    public enum biologicalGender {
        male,
        female,
        other
    }

    public struct muscleGroup {
        public string mainBodyPart;
        public int subGroup;

        public string subgroup {
            get {
                if (mainBodyPart == "Chest") {
                    if (subGroup == 1) return "Middle Chest";
                    else if (subGroup == 2) return "Upper Chest";
                    else return "Lower Chest";
                } else if (mainBodyPart == "Legs") {
                    if (subGroup == 1) return "Quads";
                    else if (subGroup == 2) return "Hamstrings";
                    else return "Calves";
                } else if (mainBodyPart == "Back") {
                    if (subGroup == 1) return "Upper Back";
                    else if (subGroup == 2) return "Lats";
                    else return "Lower Back";
                } else if (mainBodyPart == "Shoulders") {
                    if (subGroup == 1) return "Front Deltiods";
                    else if (subGroup == 2) return "Lateral Deltoids";
                    else return "Rear Deltoids/Traps";
                } else if (mainBodyPart == "Arms") {
                    if (subGroup == 1) return "Biceps";
                    else if (subGroup == 2) return "Triceps";
                    else return "Forearms";
                } else {
                    if (subGroup == 1) return "Upper Abs";
                    else if (subGroup == 2) return "Lower Abs";
                    else return "Obliques";
                }
            }
        }

        public string readable() {
            return mainBodyPart + " - " + subgroup;
        }
    }
    public struct set {
        public readonly int reps;
        public readonly int percent1RM;
        public readonly TimeSpan restTime;
        public readonly setFeedback feedback;
    }
    public struct setFeedback {
        //tier 0 feedback
        public readonly bool completed;

        //tier 1 feedback
        public readonly int difficulty;

        //tier 2 feedback
        public readonly int reps;
        public readonly int weight;
    }
    public struct workoutItem {
        public string uuid;
        public Exercise ex;
        public set[] sets;

        public string readable(int user1RM) {
            StringBuilder res = new StringBuilder();

            for(int i = 0; i < sets.Count(); i++) {
                set s = sets[i];

                string exDef = ex.exerciseName + " " + Util.percent1RM(s.percent1RM, user1RM) + "lb for " + s.reps + " " + (s.reps == 1 ? "rep" : "reps") + "\n";
                string restDef = (s.restTime.TotalSeconds < 0) ? "Immediately " : "Rest " + s.restTime.TotalSeconds + " seconds\n";

                res.Append(exDef);
                if(i != sets.Count() - 1) res.Append(restDef);
            }

            return res.ToString();
        }
    }


    public class MuscleGroupQueue {
        public readonly string mainBodyPart;

        public readonly double scalingFactor;
        public double timePutIn => totalExercises / scalingFactor;
        public int totalExercises {
            get {
                return groups.Aggregate((accumulator, initial) => accumulator + initial);
            }
        }
        public int[] groups = new int[3] { 0, 0, 0 };

        private void addToSubgroup(int group) {
            groups[group]++;
        }
        private void removeFromGroup(int group) {
            groups[group]--;
        }

        public muscleGroup generateGroupExercise() {
            int subgroup = groups[0] > groups[1] ? (groups[1] > groups[2] ? 2 : 1) : 0;
            addToSubgroup(subgroup);
            return new muscleGroup { mainBodyPart = mainBodyPart, subGroup = subgroup };
        }
        public void undoGroupExercise(muscleGroup group) {
            removeFromGroup(group.subGroup);
        }

        public MuscleGroupQueue(string mainBodyPart, double factor) {
            scalingFactor = factor;
            this.mainBodyPart = mainBodyPart;
        }
    }

    public struct planDefinition {
        public int age;
        public biologicalGender gender;
        public int experience;
        public int[] workoutTimes;
        public string[] equipmentAvailable;
        public string goal;
    }

    //TODO finish
    public class Plan {
        public HashSet<Exercise> allExercises;

        public planDefinition definition;

        public Dictionary<string, MuscleGroupQueue> groups;
        public MuscleGroupQueue chest => groups["chest"];
        public MuscleGroupQueue back => groups["back"];
        public MuscleGroupQueue legs => groups["legs"];
        public MuscleGroupQueue shoulders => groups["shoulders"];
        public MuscleGroupQueue arms => groups["arms"];
        public MuscleGroupQueue abs => groups["abs"];

        UsersPossibleExerciseList exercises = new UsersPossibleExerciseList();
        List<Exercise> myExercises = new List<Exercise>();


        //selects the style of workout for the plan 
        public string selectStyle(int experienceLevel) {
            throw new NotImplementedException();
        }

        //selects what days bodyparts will be worked out
        public string selectSplit(int workoutsPerWeek) {
            throw new NotImplementedException();
        }

        List<List<workoutItem>> pastDays;
        List<workoutItem> previousDay {
            get {
                if (pastDays.Count > 0)
                    return pastDays.Last();
                else
                    return new List<workoutItem>();
            }
        }

        string getNextGroup(HashSet<string> excludedGroups) {
            //find the lowest timePutIn of any non-excluded groups
            if (excludedGroups.Count() > 5) throw new Exception();
            double lowest = double.MaxValue;
            string lowestGroup = "";
            foreach(var kvp in groups) {
                MuscleGroupQueue g = kvp.Value;
                if(!excludedGroups.Contains(g.mainBodyPart) && g.timePutIn < lowest) {
                    lowest = g.timePutIn;
                    lowestGroup = g.mainBodyPart;
                }
            }
            
            return lowestGroup;
        }

        public List<workoutItem> generateDay(int n) {
            //determine the amount of primary and secondary exercises to do
            int primaryCount = (int)(Math.Ceiling(n * 0.666666666) + 0.5);
            int secondaryCount = n - primaryCount;


            HashSet<string> excludedGroups = new HashSet<string>();
            foreach(workoutItem item in previousDay) {
                excludedGroups.Add(item.uuid);
            }

            List<workoutItem> res = new List<workoutItem>();
            
            string primary = getNextGroup(excludedGroups);
            excludedGroups.Add(primary);
            int areaNumber = 1;
            HashSet<Exercise> allEx = allExercises;


            HashSet<Exercise> onlyCompounds = Exercise.onlycompound(allExercises);
            HashSet<Exercise> onlyCompoundsInSubgroup = Exercise.onlyCompoundsInSubgroup(allExercises, areaNumber);
            HashSet<Exercise> subgroup = Exercise.subgroup(allExercises, areaNumber);


            HashSet<Exercise> primaryAvailable = Exercise.whereAreaIs(allExercises, primary);
            for (int i = 0; i < primaryCount; i++) {
                muscleGroup group = groups[primary].generateGroupExercise();
                res.Add(new workoutItem { uuid = primary, ex = primaryAvailable.randomElement() }); //TODO: place real exercises
            }
            
            string secondary = getNextGroup(excludedGroups);
            HashSet<Exercise> secondaryAvailable = Exercise.whereAreaIs(allExercises, secondary);
            for (int i = 0; i < secondaryCount; i++) {
                muscleGroup group = groups[secondary].generateGroupExercise();
                res.Add(new workoutItem { uuid = secondary, ex = secondaryAvailable.randomElement()}); //TODO: place real exercises
            }

            pastDays.Add(res);

            return res;
        }

        public Plan(planDefinition definition, Database db) {
            this.definition = definition;
            this.pastDays = new List<List<workoutItem>>();

            groups = new Dictionary<string, MuscleGroupQueue>();
            groups.Add("chest", new MuscleGroupQueue("chest", 1.0));
            groups.Add("back", new MuscleGroupQueue("back", 1.0));
            groups.Add("legs", new MuscleGroupQueue("legs", 1.0));
            groups.Add("shoulders", new MuscleGroupQueue("shoulders", 0.8));
            groups.Add("arms", new MuscleGroupQueue("arms", 0.8));
            groups.Add("abdominals", new MuscleGroupQueue("abdominals", 0.8));

            allExercises = Exercise.getAllExercises(db);
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
