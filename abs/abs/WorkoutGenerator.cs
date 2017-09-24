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
            HashSet<Exercise> usedExercises = new HashSet<Exercise>();
            
            string primary = getNextGroup(excludedGroups);
            excludedGroups.Add(primary);
            HashSet<Exercise> allEx = allExercises;



            HashSet<Exercise> primaryExercises = Exercise.whereAreaIs(allExercises, primary);
            HashSet<Exercise> primarySubgroupOneExercises = Exercise.subgroup(primaryExercises, 1);
            HashSet<Exercise> primarySubgroupTwoExercises = Exercise.subgroup(primaryExercises, 2);
            HashSet<Exercise> primarySubgroupThreeExercises = Exercise.subgroup(primaryExercises, 3);
            HashSet<Exercise> primarySubgroupOneCompoundExercises = Exercise.onlycompound(primarySubgroupOneExercises);
            HashSet<Exercise> primarySubgroupTwoCompoundExercises = Exercise.onlycompound(primarySubgroupTwoExercises);
            HashSet<Exercise> primarySubgroupThreeCompoundExercises = Exercise.onlycompound(primarySubgroupThreeExercises);

            bool containsCompound1 = false;
            bool containsCompound2 = false;
            bool containsCompound3 = false;
            int turn = 1;

            for(int i = 0; i < primaryCount; i++) {
                muscleGroup group = groups[primary].generateGroupExercise();
                if (primarySubgroupOneExercises.Count != 0 && turn == 1  || (primarySubgroupTwoExercises.Count == 0 || primarySubgroupThreeExercises.Count == 0)) {
                    if (containsCompound1 == false && primarySubgroupOneCompoundExercises.Count != 0) {
                        usedExercises.Add(primarySubgroupOneCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupOneCompoundExercises = Exercise.getUnusedExercises(primarySubgroupOneCompoundExercises, usedExercises);
                        primarySubgroupOneExercises = Exercise.getUnusedExercises(primarySubgroupOneExercises, usedExercises);
                        containsCompound1 = true;
                        turn=2;
                    } else {
                        usedExercises.Add(primarySubgroupOneExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupOneCompoundExercises = Exercise.getUnusedExercises(primarySubgroupOneCompoundExercises, usedExercises);
                        primarySubgroupOneExercises = Exercise.getUnusedExercises(primarySubgroupOneExercises, usedExercises);
                        turn=2;
                    }
                }
                else if(primarySubgroupTwoExercises.Count != 0 && turn == 2 || (primarySubgroupOneExercises.Count == 0 || primarySubgroupThreeExercises.Count == 0)) {
                    if (containsCompound2 == false && primarySubgroupTwoCompoundExercises.Count != 0) {
                        usedExercises.Add(primarySubgroupTwoCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupTwoCompoundExercises = Exercise.getUnusedExercises(primarySubgroupTwoCompoundExercises, usedExercises);
                        primarySubgroupTwoExercises = Exercise.getUnusedExercises(primarySubgroupTwoExercises, usedExercises);
                        containsCompound2 = true;
                        turn=3;
                    } else {
                        usedExercises.Add(primarySubgroupTwoExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupTwoCompoundExercises = Exercise.getUnusedExercises(primarySubgroupTwoCompoundExercises, usedExercises);
                        primarySubgroupTwoExercises = Exercise.getUnusedExercises(primarySubgroupTwoExercises, usedExercises);
                        turn=3;
                    }
                } 
                else if (primarySubgroupThreeExercises.Count != 0 && turn == 3 || (primarySubgroupTwoExercises.Count == 0 || primarySubgroupOneExercises.Count == 0)) {
                    if (containsCompound3 == false && primarySubgroupThreeCompoundExercises.Count != 0) {
                        usedExercises.Add(primarySubgroupThreeCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupThreeCompoundExercises = Exercise.getUnusedExercises(primarySubgroupThreeCompoundExercises, usedExercises);
                        primarySubgroupThreeExercises = Exercise.getUnusedExercises(primarySubgroupThreeExercises, usedExercises);
                        containsCompound3 = true;
                        turn=1;
                    } else {
                        usedExercises.Add(primarySubgroupThreeExercises.randomElement());
                        res.Add(new workoutItem { uuid = primary, ex = usedExercises.Last() });
                        primarySubgroupThreeCompoundExercises = Exercise.getUnusedExercises(primarySubgroupThreeCompoundExercises, usedExercises);
                        primarySubgroupThreeExercises = Exercise.getUnusedExercises(primarySubgroupThreeExercises, usedExercises);
                        turn=1;
                    }
                }
            }

            containsCompound1 = false;
            containsCompound2 = false;
            containsCompound3 = false;

            string secondary = getNextGroup(excludedGroups);
            HashSet<Exercise> secondaryExercises = Exercise.whereAreaIs(allExercises, secondary);
            HashSet<Exercise> secondarySubgroupOneExercises = Exercise.subgroup(secondaryExercises, 1);
            HashSet<Exercise> secondarySubgroupTwoExercises = Exercise.subgroup(secondaryExercises, 2);
            HashSet<Exercise> secondarySubgroupThreeExercises = Exercise.subgroup(secondaryExercises, 3);
            HashSet<Exercise> secondarySubgroupOneCompoundExercises = Exercise.onlycompound(secondarySubgroupOneExercises);
            HashSet<Exercise> secondarySubgroupTwoCompoundExercises = Exercise.onlycompound(secondarySubgroupTwoExercises);
            HashSet<Exercise> secondarySubgroupThreeCompoundExercises = Exercise.onlycompound(secondarySubgroupThreeExercises);

            turn = 1;
            for (int i = 0; i < secondaryCount; i++) {
                muscleGroup group = groups[secondary].generateGroupExercise();
                if (secondarySubgroupOneExercises.Count != 0 && turn == 1 || (secondarySubgroupTwoExercises.Count == 0 || secondarySubgroupThreeExercises.Count == 0)) {
                    if (containsCompound1 == false && secondarySubgroupOneCompoundExercises.Count != 0) {
                        usedExercises.Add(secondarySubgroupOneCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupOneCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupOneCompoundExercises, usedExercises);
                        secondarySubgroupOneExercises = Exercise.getUnusedExercises(secondarySubgroupOneExercises, usedExercises);
                        containsCompound1 = true;
                        turn = 2;
                    } else {
                        usedExercises.Add(secondarySubgroupOneExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupOneCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupOneCompoundExercises, usedExercises);
                        secondarySubgroupOneExercises = Exercise.getUnusedExercises(secondarySubgroupOneExercises, usedExercises);
                        turn = 2;
                    }
                } else if (secondarySubgroupTwoExercises.Count != 0 && turn == 2 || (secondarySubgroupOneExercises.Count == 0 || secondarySubgroupThreeExercises.Count == 0)) {
                    if (containsCompound2 == false && secondarySubgroupTwoCompoundExercises.Count != 0) {
                        usedExercises.Add(secondarySubgroupTwoCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupTwoCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupTwoCompoundExercises, usedExercises);
                        secondarySubgroupTwoExercises = Exercise.getUnusedExercises(secondarySubgroupTwoExercises, usedExercises);
                        containsCompound2 = true;
                        turn = 3;
                    } else {
                        usedExercises.Add(secondarySubgroupTwoExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupTwoCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupTwoCompoundExercises, usedExercises);
                        secondarySubgroupTwoExercises = Exercise.getUnusedExercises(secondarySubgroupTwoExercises, usedExercises);
                        turn = 3;
                    }
                } else if (secondarySubgroupThreeExercises.Count != 0 && turn == 3 || (secondarySubgroupTwoExercises.Count == 0 || secondarySubgroupOneExercises.Count == 0)) {
                    if (containsCompound3 == false && secondarySubgroupThreeCompoundExercises.Count != 0) {
                        usedExercises.Add(secondarySubgroupThreeCompoundExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupThreeCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupThreeCompoundExercises, usedExercises);
                        secondarySubgroupThreeExercises = Exercise.getUnusedExercises(secondarySubgroupThreeExercises, usedExercises);
                        containsCompound3 = true;
                        turn = 1;
                    } else {
                        usedExercises.Add(secondarySubgroupThreeExercises.randomElement());
                        res.Add(new workoutItem { uuid = secondary, ex = usedExercises.Last() });
                        secondarySubgroupThreeCompoundExercises = Exercise.getUnusedExercises(secondarySubgroupThreeCompoundExercises, usedExercises);
                        secondarySubgroupThreeExercises = Exercise.getUnusedExercises(secondarySubgroupThreeExercises, usedExercises);
                        turn = 1;
                    }
                }
            }







            //HashSet<Exercise> primaryAvailable = Exercise.whereAreaIs(allExercises, primary);
            //for (int i = 0; i < primaryCount; i++) {
            //    muscleGroup group = groups[primary].generateGroupExercise();
            //    res.Add(new workoutItem { uuid = primary, ex = primaryAvailable.randomElement() }); //TODO: place real exercises
            //}

            //string secondary = getNextGroup(excludedGroups);
            //HashSet<Exercise> secondaryAvailable = Exercise.whereAreaIs(allExercises, secondary);
            //for (int i = 0; i < secondaryCount; i++) {
            //    muscleGroup group = groups[secondary].generateGroupExercise();
            //    res.Add(new workoutItem { uuid = secondary, ex = secondaryAvailable.randomElement()}); //TODO: place real exercises
            //}

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
