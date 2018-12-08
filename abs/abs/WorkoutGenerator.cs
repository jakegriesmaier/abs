using System;
using System.Collections.Generic;
using System.Linq;
using monopage;

namespace abs {
    public class WorkoutGenerator {
        private UserDataAccess user;

        private List<KeyValuePair<BodyPart, Tuple<int, int, int>>> bodyPartVolume;

        private Dictionary<Exercise, ProgressStatistics> stats = new Dictionary<Exercise, ProgressStatistics>();

        private HashSet<Exercise> exercises;

        private void CalculateBodyPartVolumes() {
            List<WorkoutSession> sessions = user.GetAllSessions();

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

            DateTime startDate = firstFound - new TimeSpan(7, 0, 0, 0, 0); //ein woch
            DateTime endDate = firstFound;
            List<WorkoutSession> relevant = sessions.Where(session => (session.date <= endDate && session.date >= startDate)).ToList();

            int[] days = new int[7]; //0 = today

            Dictionary<BodyPart, Tuple<int, int, int>> history =
                (Dictionary<BodyPart, Tuple<int, int, int>>)((BodyPart[])Enum.GetValues(typeof(BodyPart))).Select(part => new KeyValuePair<BodyPart, Tuple<int, int, int>>(part, new Tuple<int, int, int>(0,0,0)));
            if (relevant.Count > 0) {
                relevant.ForEach(sesh =>
                    sesh.workoutItems.ForEach(item => {
                            if (item.ex.areaNumber == 1) {
                                Tuple<int, int, int> val = history[item.ex.mainBodyPart];
                                history[item.ex.mainBodyPart] =
                                new Tuple<int,int,int>(item.sets.Select(set => set.reps * set.percent1RM).Aggregate((val0, val1) => val0 + val1), val.Item2, val.Item3);
                            } else if(item.ex.areaNumber == 2) {
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

            bodyPartVolume = sorted;
        }
        
        private void CalculateExerciseProgress() {
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

            foreach(var kvp in stats) {
                kvp.Value.Finish();
            }
        }

        private void CalculateExercises(BodyPart part, Tuple<int, int, int> groupNums) {
            HashSet<Exercise> res = new HashSet<Exercise>();

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

            //
            foreach (Exercise ex in g1s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg1[ex] = baseScore + score;
            }
            foreach (Exercise ex in g2s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg2[ex] = baseScore + score;
            }
            foreach (Exercise ex in g3s) {
                const double baseScore = 0.1;
                double score = stats.ContainsKey(ex) ? stats[ex].Lin_Slope * stats[ex].Lin_Significance : 0.0;
                dg3[ex] = baseScore + score;
            }

            //pick a bunch of random exercises from the subgroup sets we've found earlier
            res.UnionWith(dg1.selectN_unique(groupNums.Item1));
            res.UnionWith(dg2.selectN_unique(groupNums.Item2));
            res.UnionWith(dg3.selectN_unique(groupNums.Item3));

            exercises = res;
        }

        WorkoutSession GenSesh(int workoutSlots, HashSet<BodyPart> rejected = null) {

            CalculateBodyPartVolumes();

            CalculateExerciseProgress();

            CalculateExercises(BodyPart.Chest, new Tuple<int, int, int>(3, 3, 2));

            WorkoutSession res = new WorkoutSession();

            return res;
        }

        public WorkoutGenerator(UserDataAccess user) {
            this.user = user;


        }
    }
}
