using System;
using System.Collections.Generic;
using System.Linq;
using monopage;

namespace abs {
    using volume = Int32;
    using setCount = Int32;
    using ExerciseStatistics = Dictionary<Exercise, ProgressStatistics>;

    struct ExerciseHistory {
        public HashSet<Exercise> recentExercises; //exercises that occurred with the past set time period
        public List<Group> bodyPartVolumes; //volume of body parts from recentExercises
        public Dictionary<Exercise, ProgressStatistics> exerciseStatistics; //statatics of each exercise
    }

    public class Group {
        public BodyPart bodyPart { get; private set; }
        public Tuple<volume, volume, volume> subgroupVolumes { get; set; }
        public double totalVolume => subgroupVolumes.Item1 + subgroupVolumes.Item2 + subgroupVolumes.Item3;

        public Group(BodyPart part) {
            this.bodyPart = part;
            subgroupVolumes = new Tuple<volume, volume, volume>(0,0,0);
        }

        public Group(KeyValuePair<BodyPart, Tuple<volume, volume, volume>> part) {
            this.bodyPart = part.Key;
            this.subgroupVolumes = part.Value;
        }

    }

    public class WorkoutGenerator {
        private UserDataAccess user;
        private ExerciseHistory history;

        private ExerciseHistory CalculateBodyPartVolumes() {
            ExerciseHistory res = new ExerciseHistory();

            List<WorkoutSession> sessions = user.GetAllSessions();
            sessions.Sort((lhs, rhs) => (rhs.date - lhs.date).Ticks > 0 ? 1 : -1);

            bool anyFound = false;
            DateTime firstFound = DateTime.MinValue;
            foreach (WorkoutSession session in sessions) {
                foreach (WorkoutItem item in session.workoutItems) {
                    foreach (WorkoutSet set in item.sets) {
                        if (set.repsCompleted != 0) {
                            firstFound = session.date;
                            anyFound = true;
                            break;
                        }
                    }
                    if (anyFound) break;
                }
                if (anyFound) break;
            }

            if (anyFound) {

                DateTime startDate = firstFound - new TimeSpan(7, 0, 0, 0, 0); //ein woch
                DateTime endDate = firstFound;
                List<WorkoutSession> relevant = sessions.Where(session => (session.date <= endDate && session.date >= startDate)).ToList();

                res.recentExercises = new HashSet<Exercise>();
                relevant.ForEach(sesh => {
                    sesh.workoutItems.ForEach(item => {
                        res.recentExercises.Add(item.ex);
                    });
                });

                Dictionary<BodyPart, Tuple<int, int, int>> history = new Dictionary<BodyPart, Tuple<int, int, int>>();
                history.Add(BodyPart.Abs, new Tuple<int, int, int>(0, 0, 0));
                history.Add(BodyPart.Arms, new Tuple<int, int, int>(0, 0, 0));
                history.Add(BodyPart.Back, new Tuple<int, int, int>(0, 0, 0));
                history.Add(BodyPart.Chest, new Tuple<int, int, int>(0, 0, 0));
                history.Add(BodyPart.Legs, new Tuple<int, int, int>(0, 0, 0));
                history.Add(BodyPart.Shoulders, new Tuple<int, int, int>(0, 0, 0));
                if (relevant.Count > 0) {
                    relevant.ForEach(sesh =>
                        sesh.workoutItems.ForEach(item => {
                            if (item.ex.areaNumber == 1) {
                                Tuple<int, int, int> val = history[item.ex.mainBodyPart];
                                history[item.ex.mainBodyPart] =
                            new Tuple<int, int, int>(item.sets.Select(set => set.reps * set.percent1RM).Aggregate((val0, val1) => val0 + val1), val.Item2, val.Item3);
                            } else if (item.ex.areaNumber == 2) {
                                Tuple<int, int, int> val = history[item.ex.mainBodyPart];
                                history[item.ex.mainBodyPart] =
                            new Tuple<int, int, int>(val.Item1, item.sets.Select(set => set.reps * set.percent1RM).Aggregate((val0, val1) => val0 + val1), val.Item3);
                            } else {
                                Tuple<int, int, int> val = history[item.ex.mainBodyPart];
                                history[item.ex.mainBodyPart] =
                            new Tuple<int, int, int>(val.Item1, val.Item2, item.sets.Select(set => set.reps * set.percent1RM).Aggregate((val0, val1) => val0 + val1));
                            }
                        }
                        )
                    );
                }

                var sorted = history.ToList();
                sorted.Sort((lhs, rhs) => (lhs.Value.Item1 + lhs.Value.Item2 + lhs.Value.Item3) - (rhs.Value.Item1 + rhs.Value.Item2 + rhs.Value.Item3));

                Console.WriteLine("Body part volumes:");
                foreach (var something in sorted) {
                    Console.WriteLine(something.Key.ToString() + " = ");
                    Console.WriteLine("\t" + something.Value.Item1 + ", " + something.Value.Item2 + ", " + something.Value.Item3);
                }

                res.bodyPartVolumes = new List<Group>();
                sorted.ForEach(kvp => res.bodyPartVolumes.Add(new Group(kvp)));
            } else {
                res.bodyPartVolumes = new List<Group>();
                Util.GetStaticClassesFieldValues<BodyPart>(typeof(BodyPart)).ForEach(bp => {
                    res.bodyPartVolumes.Add(new Group(bp));
                });
                res.recentExercises = new HashSet<Exercise>();
            }
            return res;
        }

        private Dictionary<Exercise, ProgressStatistics> CalculateExerciseProgress(ExerciseHistory history) {
            Dictionary<Exercise, ProgressStatistics> stats = new Dictionary<Exercise, ProgressStatistics>();
            HashSet<Exercise> recentExercises = new HashSet<Exercise>();

            List<WorkoutSession> allSessions = user.GetAllSessions();

            foreach(WorkoutSession session in allSessions) {
                foreach(WorkoutItem item in session.workoutItems) {
                    if(stats.ContainsKey(item.ex)) {
                        stats[item.ex].AddDataPoint(session.date, item.oneRepMax);
                    } else {
                        stats.Add(item.ex, new ProgressStatistics());
                    }
                }
            }

            Console.WriteLine("Exercise scores:");
            foreach(var kvp in stats) {
                kvp.Value.Finish();
                Console.WriteLine(kvp.Key.exerciseName + " = ");
                Console.WriteLine("\tLin_Slope = " + kvp.Value.Lin_Slope + "\n\tLin_Significance = " + kvp.Value.Lin_Significance);
            }

            return stats;
        }

        /// <summary>
        /// Calculates which bodyparts will be used for the workout session
        /// </summary>
        /// <param name="rejected">a list of bodyparts that are to be removed from the possible parts selected</param>
        /// <param name="workoutSlots"></param>
        /// <returns>a list of bodyparts that are in the workout session</returns>
        private List<BodyPart> CalculateBodyParts(HashSet<BodyPart> rejected, int workoutSlots) {
            List<BodyPart> bodyParts = new List<BodyPart>();
            int numParts = CalculateNumberBodyParts(workoutSlots, Constants.NUM_BODYPARTS - rejected.Count());
            history.bodyPartVolumes.Where(group => !rejected.Contains(group.bodyPart)).OrderBy(group => group.totalVolume).ToList().ForEach(group => {
                if(bodyParts.Count() < numParts) {
                    bodyParts.Add(group.bodyPart);
                } else {
                    return; 
                }
            });

            return bodyParts;
        }

        /// <summary>
        /// Calculates the number of bodyparts to be in a workout session
        /// </summary>
        /// <param name="workoutSlots">the number of sets that are available for the workout</param>
        /// <param name="partsAvailable">the number of bodyparts that can be selected from</param>
        /// <returns></returns>
        private int CalculateNumberBodyParts(int workoutSlots, int partsAvailable) {
            if (workoutSlots <= 2 || partsAvailable <= 1) {
                return 1;
            } else if (workoutSlots >= 9 && partsAvailable >= 3) {
                return 3;
            }
            return 2;
        }

        /// <summary>
        /// determines how many sets of each subgroup for a bodypart to do based on the historical volume for each subgroup
        /// </summary>
        /// <param name="part">the bodypart that the subgroups need to be calculated for</param>
        /// <param name="numExercises">the number of available exercise for this bodypart</param>
        /// <returns>a tuple with the number of exercises for each subgroup</returns>
        private Tuple<int,int,int> CalculateSubgroups(BodyPart part, int numExercises) {
            Group muscleGroup = history.bodyPartVolumes.Where(grp => grp.bodyPart.Equals(part)).First();
            List<KeyValuePair<int, double>> sgs = new List<KeyValuePair<int, double>> {
                new KeyValuePair<int, double>(1,muscleGroup.subgroupVolumes.Item1),
                new KeyValuePair<int, double>(2,muscleGroup.subgroupVolumes.Item2),
                new KeyValuePair<int, double>(3,muscleGroup.subgroupVolumes.Item3)
            };
            sgs = sgs.OrderBy(sg => sg.Value).ToList();
            int index = 0;
            int sg1, sg2, sg3;
            sg1 = sg2 = sg3 = 0;
            for(int i = 0; i < numExercises; i++) {
                switch (sgs[index].Key) {
                    case 1:
                        sg1++;
                        break;
                    case 2:
                        sg2++;
                        break;
                    case 3:
                        sg3++;
                        break;
                }
                index = (index == 3) ? 0 : index + 1;
            }
            return new Tuple<int, int, int>(sg1, sg2, sg3); 
        }

        private List<Exercise> CalculateExercises(BodyPart part, Tuple<setCount, setCount, setCount> groupNums, ExerciseHistory history) {
            HashSet<Exercise> res = new HashSet<Exercise>();
            Dictionary<Exercise, ProgressStatistics> stats = history.exerciseStatistics;

            //first pick a random subgroup and find an exercise that is compound
            int chosenSubgroup = 1 + Util.rand(3);
            var g1c = Exercise.onlyCompoundsInSubgroup(Exercise.globalExercises, chosenSubgroup);
            Exercise chosenEx = g1c.ElementAt(Util.rand(g1c.Count() - 1));

            //get a set of exercises for each subgroup, excluding the compound one we picked first
            var g1s = Exercise.subgroup(Exercise.globalExercises, 1); if (chosenSubgroup == 1) g1s.Remove(chosenEx);
            var g2s = Exercise.subgroup(Exercise.globalExercises, 2); if (chosenSubgroup == 2) g2s.Remove(chosenEx);
            var g3s = Exercise.subgroup(Exercise.globalExercises, 3); if (chosenSubgroup == 3) g3s.Remove(chosenEx);

            //create a probability distribution for each of the exercise sets
            Distribution<Exercise> dg1 = new Distribution<Exercise>();
            Distribution<Exercise> dg2 = new Distribution<Exercise>();
            Distribution<Exercise> dg3 = new Distribution<Exercise>();

            foreach (Exercise ex in g1s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg1[ex] = Math.Max((history.recentExercises.Contains(ex) ? 0.3 : 0) + baseScore + score, 0.0);
                Console.WriteLine(ex.exerciseName + ", " + dg1[ex]);
            }
            foreach (Exercise ex in g2s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg2[ex] = Math.Max((history.recentExercises.Contains(ex) ? 0.3 : 0) + baseScore + score, 0.0);
                Console.WriteLine(ex.exerciseName + ", " + dg2[ex]);
            }
            foreach (Exercise ex in g3s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg3[ex] = Math.Max((history.recentExercises.Contains(ex) ? 0.3 : 0) + baseScore + score, 0.0);
                Console.WriteLine(ex.exerciseName + ", " + dg3[ex]);
            }

            //pick a bunch of random exercises from the subgroup sets we've found earlier
            res.UnionWith(dg1.selectN_unique(groupNums.Item1));
            res.UnionWith(dg2.selectN_unique(groupNums.Item2));
            res.UnionWith(dg3.selectN_unique(groupNums.Item3));

            Console.WriteLine("Chosen exercises:");
            foreach(Exercise ex in res) {
                Console.WriteLine(ex.exerciseName);
            }

            return res.ToList();
        }

        /// <summary>
        /// reorders the wokrout items in the workout to make sure compounds are at the beginning
        /// </summary>
        private List<WorkoutItem> OrderExercises(List<KeyValuePair<BodyPart,List<Exercise>>> parts) {
            List<WorkoutItem> items = new List<WorkoutItem>();
            //selects to mix the muscle groups exercises and put compounds at the beginning or keep all exercises for a bodypart together
            Random rand = new Random(1);
            int choice = rand.Next(1,3);
            if((choice == 1) && (parts.Count() > 1)) { //alternate muscle group exercises and all compounds first
                //1 remove compounds and place at the front of the workout
                int total = 0;
                List<Exercise> compound = new List<Exercise>();
                for(int i = 0; i < parts.Count(); i++) {
                    compound.AddRange(parts[i].Value.Where(ex => ex.isCompound).ToList());
                    List<Exercise> isolation = parts[i].Value.Where(ex => !ex.isCompound).ToList();
                    parts[i] = new KeyValuePair<BodyPart, List<Exercise>>(parts[i].Key, isolation);
                    total += isolation.Count();
                }
                compound.ForEach(cmp => items.Add(new WorkoutItem { ex = cmp }));
                //2 select from each group 1 at a time until there are none left
                int index = 0;
                while (items.Count() < total) {
                    for (int i = 0; i < parts.Count(); i++) {
                        if (index < parts[i].Value.Count()) {
                            items.Add(new WorkoutItem { ex = parts[i].Value[index] });
                        }
                    }
                    index++;
                }
            } else { //all of one muscle group and then the next...
                parts.ForEach(part => {
                    List<Exercise> ordered = part.Value.Where(ex => ex.isCompound).ToList();
                    List<Exercise> isolation = part.Value.Where(ex => !ex.isCompound).ToList();
                    ordered.AddRange(isolation);
                    ordered.ForEach(exr => {
                        items.Add(new WorkoutItem { ex = exr });
                    });
                });
            }
            return items;
        }

        /// <summary>
        /// determines the number of sets associated with each exercise in the workout
        /// </summary>
        /// <param name="items">the workout items that need to have sets added</param>
        /// <param name="exerciseDuration">the duration of each exercise interval in minutes</param>
        private List<WorkoutItem> CalculateSets(List<WorkoutItem> items, int exerciseDuration = 15) {
            items.ForEach(item => {
                item.sets = new List<WorkoutSet>();
                for (int i = 0; i < (exerciseDuration + (Constants.SET_DURATION-1))/Constants.SET_DURATION; i++) {
                    item.sets.Add(new WorkoutSet());
                }
            });
            return items;
        }

        /// <summary>
        /// determines the number of reps associated with each set in the exercise in the workout
        /// </summary>
        private List<WorkoutItem> CalculateReps(List<WorkoutItem> items) {
            items.ForEach(item => {
                int numSets = item.sets.Count();
                List<int> style = SelectStyle(numSets);
                for(int i = 0; i < item.sets.Count(); i++) {
                    WorkoutSet set = item.sets[i];
                    set.reps = style[i];
                    item.sets[i] = set;
                }
            });

            return items;
        }

        /// <summary>
        /// selects the rep organization for a given workout
        /// </summary>
        /// <returns>the number of reps to do for each set</returns>
        private List<int> SelectStyle(int sets) {
            List<int> res = new List<int>();
            Random rand = new Random();
            if(sets == 5) {
                //chooses between the five set options with differing probabilities
                int choice = rand.Next(0,Constants.SetStyles.FiveSetOptions.NUM_OPTIONS * 2);
                if(choice == 0) {
                    res = Util.GetStaticClassesFieldValues<int>(typeof(Constants.SetStyles.FiveSetOptions.FiveByFifteen));
                } else if(choice == 1) {
                    res = Util.GetStaticClassesFieldValues<int>(typeof(Constants.SetStyles.FiveSetOptions.FiveSetPyramid));
                } else if(choice > 1 && choice < 4) {
                    res = Util.GetStaticClassesFieldValues<int>(typeof(Constants.SetStyles.FiveSetOptions.FiveByFive));
                } else if(choice > 3 & choice < 7) {
                    res = Util.GetStaticClassesFieldValues<int>(typeof(Constants.SetStyles.FiveSetOptions.FiveByTen));
                } else {
                    res = Util.GetStaticClassesFieldValues<int>(typeof(Constants.SetStyles.FiveSetOptions.FiveSetLadder));
                }
            }else {
                //default that adds sets of 10 reps
                for(int i = 0; i < sets; i++) {
                    res.Add(10);
                }
            }
            return res;
        }

        /// <summary>
        /// determines the percentage of one rep max for each set in the workout
        /// </summary>
        private List<WorkoutItem> CalculateIntensity(List<WorkoutItem> items, List<BodyPart> partsList) {
            Dictionary<BodyPart, int> numSetsByBodyPart = new Dictionary<BodyPart, volume>();
            partsList.ForEach(part => numSetsByBodyPart.Add(part,0));
            items.ForEach(item => numSetsByBodyPart[item.ex.mainBodyPart] += item.sets.Count());
            Dictionary<BodyPart, int> bodyPartSetProgress = new Dictionary<BodyPart, volume>();
            partsList.ForEach(part => bodyPartSetProgress.Add(part, 0));
            for (int i = 0; i < items.Count(); i++) {
                for(int j = 0; j < items[i].sets.Count(); j++) {
                    BodyPart bodyPart = items[i].ex.mainBodyPart;
                    bodyPartSetProgress[bodyPart] += 1;
                    WorkoutSet set = items[i].sets[j];
                    set.percent1RM = CalculatePercentOfMax(items[i].sets.Count(), j, set.reps, numSetsByBodyPart[bodyPart], bodyPartSetProgress[bodyPart]);
                }
            }
            //use this as the base Util.repsMaxPercent1RM() and then look at location in workout and exercise style
            return items;
        }

        /// <summary>
        /// calculates the percentage of the one rep max that should be performed
        /// </summary>
        /// <param name="numSetsExercise">the number of sets for the exercise</param>
        /// <param name="setNumberExercise">the current sets location in the workout</param>
        /// <param name="numReps">the number of reps for the current set</param>
        /// <param name="numBodyPartSets">the number of sets for this set's bodypart</param>
        /// <param name="bodyPartSetNumber">the current sets location in the workout in terms of its bodypart</param>
        /// <returns>the percent of the total one rep max that should be performed</returns>
        private int CalculatePercentOfMax(int numSetsExercise, int setNumberExercise, int numReps, double numBodyPartSets, double bodyPartSetNumber) {
            int percent = Util.repsMaxPercent1RM(numReps);//gets the maximum possible percent for that number of reps
            if ((bodyPartSetNumber / numBodyPartSets) >= .9) {//need to add in volume curve? or will this effect the individual exercise progression?
                percent -= 2;
            }
            if (bodyPartSetNumber == 1) {
                percent -= 5;//warmup set
            }
            int exerciseProgress = numSetsExercise - setNumberExercise;
            percent -= exerciseProgress + 1;
            return Math.Max(0,percent);
        }

        /// <summary>
        /// Calculates the total number of sets that are in a list of workout items
        /// </summary>
        private int CalculateTotalSets(List<WorkoutItem> items) {
            int total = 0;
            items.ForEach(item => total+=item.sets.Count());
            return total;
        }

        /// <summary>
        /// sets the restime for each set in the workout
        /// </summary>
        private List<WorkoutItem> CalculateRestime(List<WorkoutItem> items) {
            items.ForEach(item => item.sets.ForEach(set => set.restTime = TimeSpan.FromSeconds(60)));
            return items;
        }

        public WorkoutSession GenSesh(int workoutSlots, HashSet<BodyPart> rejected = null) {
            WorkoutSession workout = new WorkoutSession();

            workout.bodyparts = CalculateBodyParts(rejected, workoutSlots);

            List<KeyValuePair<BodyPart, Tuple<int, int, int>>> bodyPartSubgroups = new List<KeyValuePair<BodyPart, Tuple<int, int, int>>>();
            int[] distribution = new int[workout.bodyparts.Count()];
            int index = 0;
            for(int i = 0; i < workoutSlots; i++) {
                distribution[index]++;
                index = (index == distribution.Count() + 1) ? 0 : (index + 1);
            }

            for (int i = 0; i < workout.bodyparts.Count(); i++) {
                BodyPart part = workout.bodyparts[i];
                bodyPartSubgroups.Add(new KeyValuePair<BodyPart, Tuple<volume, volume, volume>>(part, CalculateSubgroups(part,distribution[i])));
            }

            //selects the exercises for the workout
            List<KeyValuePair<BodyPart, List<Exercise>>> exercises = new List<KeyValuePair<BodyPart, List<Exercise>>>();
            bodyPartSubgroups.ForEach(group => {
                exercises.Add(new KeyValuePair<BodyPart, List<Exercise>>(group.Key,CalculateExercises(group.Key, group.Value, history))); 
            });

            List<WorkoutItem> items = new List<WorkoutItem>();
            items = OrderExercises(exercises);

            items = CalculateSets(items);

            items = CalculateReps(items);//depends on workout style???

            items = CalculateIntensity(items, workout.bodyparts);//depends on number of reps and location in the workout

            items = CalculateRestime(items);

            workout.workoutItems = items;
            return workout;
        }

        /// <summary>
        /// initialized and calculates all of the values for the users history
        /// </summary>
        public void BuildWorkoutHistory() {
            history = new ExerciseHistory();
            history = CalculateBodyPartVolumes();
            history.exerciseStatistics = CalculateExerciseProgress(history);
        }

        public WorkoutGenerator(UserDataAccess user) {
            this.user = user;
            BuildWorkoutHistory();
        }
    }
}
