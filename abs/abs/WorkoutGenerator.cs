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
        public int reps;
        public int percent1RM;
        public TimeSpan restTime;
        public setFeedback feedback;
        public bool completed => feedback.completed;
    }
    public struct setFeedback {
        //tier 0 feedback
        public bool completed;

        //tier 1 feedback
        public int difficulty;

        //tier 2 feedback
        public int reps;
        public int weight;
    }
    public struct workoutItem {
        public string uuid;
        public Exercise ex;
        public List<set> sets;
        public bool completed { get { foreach (set s in sets) { if (!s.completed) return false; } return true; } }

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


        public string title() {
            return "<div id='" + uuid + "' class='mpExerciseTitle' ytLink='" + ex.youtube + "' completed='" + completed + "'>" + ex.exerciseName + "</div>";
        }
        public string html(int user1RM) {
            StringBuilder res = new StringBuilder();

            for (int i = 0; i < sets.Count(); i++) {
                set s = sets[i];

                string box = "<div style='background-color: red; display: inline-block; width: 10vw; height: 10vh; float: left;'> test </div>";

                string exDef = "<div>" + ex.exerciseName + " " + Util.percent1RM(s.percent1RM, user1RM) + "lb for " + s.reps + " " + (s.reps == 1 ? "rep" : "reps") + "</div><br>";
                string restDef = "<div>" + ((s.restTime.TotalSeconds < 0) ? "Immediately " : "Rest ") + s.restTime.TotalSeconds + " seconds </div><br>";

                res.Append(box);
                //res.Append(exDef);
                //if (i != sets.Count() - 1) res.Append(restDef);
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
            groups[group - 1]++;
        }
        private void removeFromGroup(int group) {
            groups[group - 1]--;
        }

        public void reset() {
            groups[0] = 0;
            groups[1] = 0;
            groups[2] = 0;
        }

        public muscleGroup generateGroupExercise() {
            int subgroup = Array.IndexOf(groups, groups.Min()) + 1;
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
        public int[] workoutTimes;
        public List<string> equipmentAvailable;
        public char goal;
    }

    public struct WorkoutDay {
        public List<workoutItem> workoutItems;
        public string primaryGroup;
        public string secondaryGroup;
        public DateTime date;
    }

    //TODO finish
    public class Plan {
        public HashSet<Exercise> allExercises;
        public HashSet<WorkoutDay> oldItems;
        public User user;
        public Database dat;

        public planDefinition definition;

        public Dictionary<string, MuscleGroupQueue> groups;
        public MuscleGroupQueue chest => groups["chest"];
        public MuscleGroupQueue back => groups["back"];
        public MuscleGroupQueue legs => groups["legs"];
        public MuscleGroupQueue shoulders => groups["shoulders"];
        public MuscleGroupQueue arms => groups["arms"];
        public MuscleGroupQueue abs => groups["abdominals"];

        public Plan(Database dat, User user) {
            this.user = user;
            this.dat = dat;

            this.pastDays = new List<List<workoutItem>>();

            groups = new Dictionary<string, MuscleGroupQueue>();
            groups.Add("chest", new MuscleGroupQueue("chest", 1.0));
            groups.Add("back", new MuscleGroupQueue("back", 1.0));
            groups.Add("legs", new MuscleGroupQueue("legs", 1.0));
            groups.Add("shoulders", new MuscleGroupQueue("shoulders", 0.8));
            groups.Add("arms", new MuscleGroupQueue("arms", 0.8));
            groups.Add("abdominals", new MuscleGroupQueue("abdominals", 0.8));

            allExercises = Exercise.getAllExercises(dat);

            Dictionary<string, List<binaryData>> res = dat.query("SELECT * FROM plans WHERE associateduser='" + user.username + "';");

            if(res.First().Value.Count == 0) {
                this.definition = new planDefinition {
                    workoutTimes = new int[] { 0, 0, 0, 0, 0, 0, 0 },
                    equipmentAvailable = new List<string> { },
                    goal = 'm'
                };

                return;

            } else {
                this.definition = new planDefinition {
                    workoutTimes = res["workouttimes"].First().asString().Split(',').Select(str => int.Parse(str)).ToArray(),
                    equipmentAvailable = res["equipmentavailable"].First().asString().Split(',').ToList(),
                    goal = res["goal"].First().asString().First()//need as char in monopage @quinn :(
               };
            }

            Dictionary<string, List<binaryData>> days = dat.query("SELECT * FROM workoutdays WHERE associateduser='" + user.username + "';");

            for(int i =  0; i < days.First().Value.Count; i++) {
                this.oldItems.Add(new WorkoutDay { workoutItems = new List<workoutItem> { },
                    primaryGroup = days["primarygroup"][i].asString(),
                    secondaryGroup = days["secondarygroup"][i].asString(),
                    date =  DateTime.Parse(days["workoutdate"][i].asString())//add date time to monopage
                });
            }

            

        }

        static void store() {

        }


        //selects the style of workout for the plan 
        public string selectStyle(int experienceLevel) {
            throw new NotImplementedException();
        }

        //selects what days bodyparts will be worked out
        public string selectSplit(int workoutsPerWeek) {
            throw new NotImplementedException();
        }

        public List<List<workoutItem>> pastDays;
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

        

        //pass in primary or secondary exercise list
        public void exercisesToUse(HashSet<Exercise> subgroup, HashSet<Exercise> compounds, HashSet<Exercise> usedExercises, List<workoutItem> res, bool containsComp, string pOrS) {
            if (containsComp == false && compounds.Count != 0) {
                usedExercises.Add(compounds.randomElement());
                res.Add(new workoutItem { uuid = pOrS, ex = usedExercises.Last() });
                containsComp = true;
            } else {
                usedExercises.Add(subgroup.randomElement());
                res.Add(new workoutItem { uuid = pOrS, ex = usedExercises.Last() });
            }
            
            compounds = Exercise.getUnusedExercises(compounds, usedExercises);
            subgroup = Exercise.getUnusedExercises(subgroup, usedExercises);
        }

        public List<workoutItem> generateDay(int n) {
            //determine the amount of primary and secondary exercises to do
            int primaryCount = (int)(Math.Ceiling(n * 0.666666666) + 0.5);
            int secondaryCount = n - primaryCount;


            HashSet<string> excludedGroups = new HashSet<string>();
            foreach(workoutItem item in previousDay) {
                excludedGroups.Add(item.ex.mainBodyPart);
            }

            List<workoutItem> res = new List<workoutItem>();
            HashSet<Exercise> usedExercises = new HashSet<Exercise>();
            
            string primary = getNextGroup(excludedGroups);
            excludedGroups.Add(primary);
            string secondary = getNextGroup(excludedGroups);

            HashSet<Exercise> available = allExercises;

            //remove exercises that we don't have equipment for
            //available = Exercise.availableWithEquipment(available, equipmentAvailable, equipmentAvailable2, weightRequired);

            //remove exercises that aren't part of the main area
            HashSet<Exercise> exercisesInPrimaryGroup = Exercise.whereAreaIs(available, primary);
            HashSet<Exercise> exercisesInSecondaryGroup = Exercise.whereAreaIs(available, secondary);
            

            for (int u = 0; u < 2; u++) {
                bool isSecondary = u != 0;
                string section = isSecondary ? secondary : primary;
                int count = isSecondary ? secondaryCount : primaryCount;

                bool[] firstOfSubgroup = new bool[3] { true, true, true };

                HashSet<Exercise> sectionExercies = isSecondary ? exercisesInSecondaryGroup : exercisesInPrimaryGroup;

                for (int i = 0; i < count; i++) {
                    muscleGroup group = groups[section].generateGroupExercise();

                    HashSet <Exercise> exercisesInCurrentSubgroup = Exercise.subgroup(sectionExercies, group.subGroup);

                    Exercise selectedExercise = null;

                    if (firstOfSubgroup[group.subGroup - 1]) {
                        firstOfSubgroup[group.subGroup - 1] = false;
                        HashSet<Exercise> compoundExercisesInCurrentSubgroup = Exercise.onlycompound(exercisesInCurrentSubgroup);

                        if (compoundExercisesInCurrentSubgroup.Count == 0) {
                            selectedExercise = exercisesInCurrentSubgroup.randomElement();
                        } else {
                            selectedExercise = compoundExercisesInCurrentSubgroup.randomElement();
                        }
                    } else {
                        selectedExercise = exercisesInCurrentSubgroup.randomElement();
                    }

                    sectionExercies.Remove(selectedExercise);


                    res.Add(new workoutItem {
                        uuid = Guid.NewGuid().ToString(),
                        ex = selectedExercise,
                        sets = new List<set> {
                            new set {
                                reps = 10,
                                percent1RM = 70,
                                restTime = new TimeSpan(0, 0, 3)
                            },
                            new set {
                                reps = 12,
                                percent1RM = 70,
                                restTime = new TimeSpan(0, 0, 45)
                            },
                            new set {
                                reps = 15,
                                percent1RM = 70,
                                restTime = new TimeSpan(0, 0, 30)
                            }
                        }
                    });
                }
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
