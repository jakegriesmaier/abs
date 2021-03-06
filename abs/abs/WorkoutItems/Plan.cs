﻿using System;
using System.Collections.Generic;
using System.Linq;
using monopage;

namespace abs {
    //TODO finish
    public class Plan {
        public HashSet<Exercise> allExercises;
        public List<WorkoutDay> oldDays;
        public User user;
        public Database dat;
        public Dictionary<string, int> exercise1rms;

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

            groups = new Dictionary<string, MuscleGroupQueue>();
            groups.Add("chest", new MuscleGroupQueue("chest", 1.0));
            groups.Add("back", new MuscleGroupQueue("back", 1.0));
            groups.Add("legs", new MuscleGroupQueue("legs", 1.0));
            groups.Add("shoulders", new MuscleGroupQueue("shoulders", 0.8));
            groups.Add("arms", new MuscleGroupQueue("arms", 0.8));
            groups.Add("abdominals", new MuscleGroupQueue("abdominals", 0.8));

            allExercises = Exercise.getAllExercises(dat);
            

            QueryResult days = dat.query("SELECT * FROM workoutdays WHERE associateduser='" + user.email + "';");

            oldDays = new List<WorkoutDay>();

            for (int i = 0; i < days.Rows; i++) {
                this.oldDays.Add(new WorkoutDay {
                    workoutItems = new List<workoutItem> { },
                    primaryGroup = days.GetField("primarygroup", i).asString(),
                    secondaryGroup = days.GetField("secondarygroup", i).asString(),
                    date = days.GetField("workoutdate", i).asDate(),
                    uuid = days.GetField("dayid", i).asString()
            });                    
            }

            foreach(WorkoutDay d in oldDays) {
                QueryResult items = dat.query("SELECT * FROM workoutitems WHERE associatedday ='" + d.uuid +"';");

                for(int i = 0; i < items.Rows; i++) {
                    Exercise exercise = null;
                    String uuid = items.GetField("itemid", i).asString();

                    foreach (Exercise ex in allExercises) {
                        if (ex.exerciseName == items.GetField("exercisename", i).asString()) {
                            exercise = ex;
                        }
                    }

                    QueryResult setQuery = dat.query("SELECT * FROM workoutsets WHERE associateditem ='" + uuid + "';");
                    List<set> exerciseSets = new List<set>();
                    for(int j = 0; j < setQuery.Rows; j++) {
                        exerciseSets.Add(new set {
                            percent1RM = setQuery.GetField("percent1rm", j).asInt(),
                            reps = setQuery.GetField("reps", j).asInt(),
                            restTime = TimeSpan.FromSeconds((float)setQuery.GetField("resttime", j).asDouble()),
                            repsCompleted = setQuery.GetField("feedbackreps", j).asInt()
                        });
                    }

                    d.workoutItems.Add(new workoutItem {
                        uuid = uuid,
                        ex = exercise,
                        sets = exerciseSets,
                        difficulty = items.GetField("feedbackdifficulty", i).asInt()
                    });
                }
            }
        }

        
        public void store(List<WorkoutDay> newDays) {
            foreach (WorkoutDay day in newDays) {
                string dayid = Guid.NewGuid().ToString();
                string associatedUser = user.email;
                string workoutdate = day.date.ToString("yyyy-MM-dd");
                string primarygroup = day.primaryGroup;
                string secondarygroup = day.secondaryGroup;
                int itemcount = day.workoutItems.Count();
                dat.query(String.Format("INSERT INTO workoutdays VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', {5});",
                    dayid, associatedUser, workoutdate, primarygroup, secondarygroup, itemcount));

                foreach (workoutItem item in day.workoutItems) {
                    string itemid = item.uuid;
                    string associatedday = dayid;
                    string exercisename = item.ex.exerciseName;
                    int setcount = item.sets.Count();
                    int difficulty = item.difficulty;
                    dat.query(String.Format("INSERT INTO workoutitems VALUES ('{0}', '{1}', '{2}', {3}, {4});",
                        itemid, associatedday, exercisename, setcount, difficulty));

                    foreach (set s in item.sets) {
                        string associateditem = item.uuid;
                        int reps = s.reps;
                        int percent1rm = s.percent1RM;
                        float resttime = (float)s.restTime.TotalSeconds;
                        int feedbackreps = s.repsCompleted;

                        dat.query(String.Format("INSERT INTO workoutsets VALUES ('{0}', {1}, {2}, {3}, {4});",
                            associateditem, reps, percent1rm, resttime, feedbackreps));
                    }
                }
            }
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
        /// <summary>
        /// selects the style of workout for the plan
        /// </summary>
        /// <param name="experienceLevel"></param>
        /// <returns></returns>
        public string selectStyle(int experienceLevel) {
            throw new NotImplementedException();
        }

        //selects what days bodyparts will be worked out
        public string selectSplit(int workoutsPerWeek) {
            throw new NotImplementedException();
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
                workoutItems = new List<workoutItem>(),
                date = DateTime.Now
            };

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

                    SetDetails details = SelectSetDetails(5);//5 sets per exercises is hardcoded in (should change this)
                    List<set> itemSets = new List<set>();
                    foreach(SetDetail s in details.sets) {
                        itemSets.Add(new set {
                            reps = s.reps,
                            percent1RM = s.Percent1RM,
                            restTime = TimeSpan.FromSeconds(s.restTime)
                        });
                    }

                    res.workoutItems.Add(new workoutItem {
                        uuid = Guid.NewGuid().ToString(),
                        ex = selectedExercise,
                        sets = itemSets
                    });
                }
            }
            
            return res;
        }

        public workoutItem findItem(string uuid) {
            for(int i = 0; i < oldDays.Count; i++) {
                for(int u = 0; u < oldDays[i].workoutItems.Count; u++) {
                    if(oldDays[i].workoutItems[u].uuid == uuid) {
                        return oldDays[i].workoutItems[u];
                    }
                }
            }
            throw new Exception("couldn't find workout item");
        }
    }

}
