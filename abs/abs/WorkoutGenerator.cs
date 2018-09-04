using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monopage;

namespace abs {
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

    public class WorkoutGenerator {
        public Dictionary<string, MuscleGroupQueue> groups;

        private UserDataAccess access;

        public WorkoutGenerator(UserDataAccess access) {
            this.access = access;

            groups = new Dictionary<string, MuscleGroupQueue>();
            groups.Add("chest", new MuscleGroupQueue("chest", 1.0));
            groups.Add("back", new MuscleGroupQueue("back", 1.0));
            groups.Add("legs", new MuscleGroupQueue("legs", 1.0));
            groups.Add("shoulders", new MuscleGroupQueue("shoulders", 0.8));
            groups.Add("arms", new MuscleGroupQueue("arms", 0.8));
            groups.Add("abdominals", new MuscleGroupQueue("abdominals", 0.8));
        }

        /// <summary>
        /// Selects Set Details depending on the number of sets in a workout item (3 or 5 sets)
        /// </summary>
        /// <param name="numSets"></param>
        /// <returns></returns>
        public SetDetails SelectSetDetails(int numSets) {
            Random rnd = new Random();
            int randNum = 0;
            List<SetDetails> details = new List<SetDetails>();
            if(numSets == 5) {
                Styles.FiveSetStyles.ForEach(s => details.AddRange(Enumerable.Repeat(s,s.weightedLikelihood)));
            } else if(numSets == 3) {
                Styles.FiveSetStyles.ForEach(s => details.AddRange(Enumerable.Repeat(s, s.weightedLikelihood)));            
            }
            if(details.Count > 0) {
                randNum = rnd.Next(details.Count);
                return details[randNum];
            }
            return new SetDetails();
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

        public WorkoutDay generateDay(int n) {
            //determine the amount of primary and secondary exercises to do
              int primaryCount = (int)(Math.Ceiling(n * 0.666666666) + 0.5);
            int secondaryCount = n - primaryCount;


            HashSet<string> excludedGroups = new HashSet<string>(); //note: eliminate this later on

            HashSet<Exercise> usedExercises = new HashSet<Exercise>();
            
            string primary = getNextGroup(excludedGroups);
            excludedGroups.Add(primary);
            string secondary = getNextGroup(excludedGroups);

            WorkoutDay res = new WorkoutDay { uuid = Guid.NewGuid().ToString(),
                primaryGroup = primary,
                secondaryGroup = secondary,
                workoutItems = new List<WorkoutItem>(),
                date = DateTime.Now
            };

            HashSet<Exercise> available = Exercise.globalExercises;

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

                    SetDetails details = SelectSetDetails(5);//5 sets per exercises is hardcoded in (should change this)
                    List<WorkoutSet> itemSets = new List<WorkoutSet>();
                    foreach(SetDetail s in details.sets) {
                        itemSets.Add(new WorkoutSet {
                            uuid = Guid.NewGuid().ToString(),
                            reps = s.reps,
                            percent1RM = s.Percent1RM,
                            restTime = TimeSpan.FromSeconds(s.restTime),
                            repsCompleted = -1
                        });
                    }

                    res.workoutItems.Add(new WorkoutItem {
                        uuid = Guid.NewGuid().ToString(),
                        ex = selectedExercise,
                        sets = itemSets,
                        oneRepMax = CalculateOneRepMax(selectedExercise)
                    });
                }
            }
            
            return res;
        }

        private double CalculateOneRepMax(Exercise ex) {
            var relevantItems = access.GetItemsWithExercise(ex);
            if(relevantItems.Count == 0) {
                var calibration = access.GetMostRecentCalibratedOneRepMax(ex.exerciseName);
                if (calibration.Exists) {
                    return calibration.Value;
                } else {
                    return -1.0;
                }
            } else {
                var item = relevantItems.Last();
                if(item.difficulty == 0 || item.difficulty == 2) {
                    return item.oneRepMax + 5;
                } else if(item.difficulty == 1) {
                    return Math.Max(item.difficulty - 5, 5);
                } else {
                    return -1.0;
                }
            }
        }
    }


    /// <summary>
    /// Contains the reps and percent1RM for a set
    /// </summary>
    public struct SetDetail{
        public int reps;
        public int Percent1RM;
        public int restTime;
    }

    /// <summary>
    /// Contains specifications for a workout item and the probability of selection
    /// </summary>
    public struct SetDetails {
        public int weightedLikelihood;
        public SetDetail[] sets;
    }

    /// <summary>
    /// Contains the different styles of exercise sets that can be performed (Contains percent1RMs and reps)
    /// </summary>
    public static class Styles {

        #region SetTypes
        public static readonly SetDetail TenRepSet = new SetDetail { reps = 10, Percent1RM = 71, restTime = 60 };
        public static readonly SetDetail FiveRepSet = new SetDetail { reps = 5, Percent1RM = 81, restTime = 90 };
        public static readonly SetDetail FifteenRepSet = new SetDetail { reps = 15, Percent1RM = 64, restTime = 45 };
        public static readonly SetDetail DeloadSet = new SetDetail { reps = 10, Percent1RM = 64, restTime = 60 };
        #endregion

        public static readonly SetDetail[] FiveByTen = { TenRepSet, TenRepSet, TenRepSet, TenRepSet, TenRepSet };
        public static readonly SetDetail[] FiveByFive = { FiveRepSet, FiveRepSet, FiveRepSet, FiveRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveByFifteen = { FifteenRepSet, FifteenRepSet, FifteenRepSet, FifteenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FivePyramid = { FifteenRepSet, TenRepSet, FiveRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FiveReversePyramid = { FiveRepSet, TenRepSet, FifteenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveLatter = { FiveRepSet, FiveRepSet, TenRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FiveReverseLatter = { FifteenRepSet, FifteenRepSet, TenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveDeload = { DeloadSet, DeloadSet, DeloadSet, DeloadSet, DeloadSet };

        public static readonly SetDetail[] ThreeByTen = { TenRepSet, TenRepSet, TenRepSet };
        public static readonly SetDetail[] ThreeByFive = { FiveRepSet, FiveRepSet, FiveRepSet };
        public static readonly SetDetail[] ThreeByFifteen = { FifteenRepSet, FifteenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreePyramid = { TenRepSet, FifteenRepSet, TenRepSet };
        public static readonly SetDetail[] ThreeReversePyramid = { FifteenRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreeLatter = { FiveRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreeReverseLatter = { FifteenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] ThreeDeload = { DeloadSet, DeloadSet, DeloadSet };

        #region FiveSetters
        public static readonly List<SetDetails> FiveSetStyles = new List<SetDetails>{
            new SetDetails{ sets = FiveByTen, weightedLikelihood = 10},
            new SetDetails{ sets = FiveByFive, weightedLikelihood = 3},
            new SetDetails{ sets = FiveByFifteen, weightedLikelihood = 3},
            new SetDetails{ sets = FivePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = FiveReversePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = FiveLatter, weightedLikelihood = 3},
            new SetDetails{ sets = FiveReverseLatter, weightedLikelihood = 3},
            new SetDetails{ sets = FiveDeload, weightedLikelihood = 1}
        };
        
        #endregion

        #region ThreeSetters
        public static readonly List<SetDetails> ThreeSetStyles = new List<SetDetails>{
            new SetDetails{ sets = ThreeByTen, weightedLikelihood = 10},
            new SetDetails{ sets = ThreeByFive, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeByFifteen, weightedLikelihood = 3},
            new SetDetails{ sets = ThreePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeReversePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeLatter, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeReverseLatter, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeDeload, weightedLikelihood = 1}
        };
        #endregion

    }
}
