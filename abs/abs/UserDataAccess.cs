using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using monopage;


namespace abs {
    public class UserDataAccess : IDisposable {
        public User user { get; private set; }
        private Database db;


        private Dictionary<string, List<Calibration>> user1rms = new Dictionary<string, List<Calibration>>();
        private Dictionary<string,List<Calibration>> new1rms = new Dictionary<string, List<Calibration>>();
        private void LoadAllCalibrations() {
            var data = db.query($"SELECT * FROM userinfo WHERE email = '{user.email}'");
            for (int i = 0; i < data.Rows; i++) {
                var row = data.GetRow(i);
                AddLoadedCalibration(row[1].asString(), row[2].asInt(), row[3].asInt(), row[4].asDate());
            }
        }
        private void StoreAllCalibrations() {
            foreach (var exs in new1rms) {
                foreach (var ex in exs.Value) {
                    db.query($"INSERT INTO usercalibration  VALUES ('{user.email}','{exs.Key}', {ex.reps}, {ex.weight},'{Util.DateStringFormat(ex.recorded.Value)}')");
                }
                exs.Value.Clear();
            }
        }
        private void AddLoadedCalibration(string exercise, int reps, int weight, DateTime date) {
            Calibration oneRepMax = new Calibration { reps = reps, weight = weight, recorded = date };

            if (!user1rms.ContainsKey(exercise)) {
                user1rms.Add(exercise, new List<Calibration>());
            }
            user1rms[exercise].Add(oneRepMax);
        }
        public void IngestCalibrationInfo(string exercise, int reps, int weight) {
            Calibration oneRepMax = new Calibration { reps = reps, weight = weight, recorded = DateTime.Now };

            if (!user1rms.ContainsKey(exercise)) {
                user1rms.Add(exercise, new List<Calibration>());
            }
            user1rms[exercise].Add(oneRepMax);

            if (!new1rms.ContainsKey(exercise)) {
                new1rms.Add(exercise, new List<Calibration>());
            }
            new1rms[exercise].Add(oneRepMax);
        }
        public Calibration GetMostRecentCalibratedOneRepMax(string exercise) {
            if(user1rms.ContainsKey(exercise)) {
                List<Calibration> oneRMs = user1rms[exercise];
                //return oneRMs?.Last() ?? new OneRepMax { reps = -1, weight = -1, recorded = null };
                return (oneRMs != null) ? oneRMs.Last() : new Calibration { reps = -1, weight = -1, recorded = null };
            } else {
                return new Calibration { reps = -1, weight = -1, recorded = null };
            }
        }


        private List<WorkoutSession> oldDays = new List<WorkoutSession>();
        private List<WorkoutSession> newDays = new List<WorkoutSession>();
        private List<WorkoutItem> updatedItems = new List<WorkoutItem>();
        private void LoadWorkoutsAfter(DateTime start) {
            QueryResult days = db.query("SELECT * FROM workoutdays WHERE associateduser='" + user.email + "' AND workoutdate>='" + Util.DateStringFormat(start) + "';");

            for (int i = 0; i < days.Rows; i++) {
                this.oldDays.Add(new WorkoutSession {
                    workoutItems = new List<WorkoutItem> { },
                    primaryGroup = days.GetField("primarygroup", i).asString(),
                    secondaryGroup = days.GetField("secondarygroup", i).asString(),
                    date = days.GetField("workoutdate", i).asDate(),
                    uuid = days.GetField("uuid", i).asString()
                });
            }

            foreach (WorkoutSession d in oldDays) {
                QueryResult items = db.query("SELECT * FROM workoutitems WHERE associatedday ='" + d.uuid + "';");

                for (int i = 0; i < items.Rows; i++) {
                    Exercise exercise = null;
                    string uuid = items.GetField("uuid", i).asString();
                    string exercisename = items.GetField("exercisename", i).asString();

                    exercise = Exercise.getByName(exercisename);

                    QueryResult setQuery = db.query("SELECT * FROM workoutsets WHERE associateditem ='" + uuid + "';");
                    List<WorkoutSet> exerciseSets = new List<WorkoutSet>();
                    for (int j = 0; j < setQuery.Rows; j++) {
                        exerciseSets.Add(new WorkoutSet {
                            uuid = setQuery.GetField("uuid", j).asString(),
                            percent1RM = setQuery.GetField("percent1rm", j).asInt(),
                            reps = setQuery.GetField("reps", j).asInt(),
                            restTime = TimeSpan.FromSeconds((float)setQuery.GetField("resttime", j).asDouble()),
                            repsCompleted = setQuery.GetField("feedbackreps", j).asInt()
                        });
                    }

                    d.workoutItems.Add(new WorkoutItem {
                        uuid = uuid,
                        ex = exercise,
                        sets = exerciseSets,
                        difficulty = items.GetField("difficulty", i).asInt()
                    });
                }
            }
        }
        private void StoreAllWorkouts() {
            StringBuilder command = new StringBuilder();


            foreach (WorkoutSession day in newDays) {
                string dayid = Guid.NewGuid().ToString();
                string associatedUser = user.email;
                string workoutdate = day.date.ToString("yyyy-MM-dd");
                string primarygroup = day.primaryGroup;
                string secondarygroup = day.secondaryGroup;
                int itemcount = day.workoutItems.Count();

                command.AppendLine(
                    String.Format("INSERT INTO workoutdays VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', {5});",
                    dayid, associatedUser, workoutdate, primarygroup, secondarygroup, itemcount));

                foreach (WorkoutItem item in day.workoutItems) {
                    string itemid = item.uuid;
                    string associatedday = dayid;
                    string exercisename = item.ex.exerciseName;
                    int setcount = item.sets.Count();
                    int difficulty = item.difficulty;
                    
                    command.AppendLine(String.Format("INSERT INTO workoutitems VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5});",
                        itemid, associatedday, exercisename, setcount, difficulty, item.oneRepMax));

                    foreach (WorkoutSet s in item.sets) {
                        string uuid = s.uuid;
                        string associateditem = item.uuid;
                        int reps = s.reps;
                        int percent1rm = s.percent1RM;
                        float resttime = (float)s.restTime.TotalSeconds;
                        int feedbackreps = s.repsCompleted;

                        command.AppendLine(String.Format("INSERT INTO workoutsets VALUES ('{0}', '{1}', {2}, {3}, {4}, {5});",
                            uuid, associateditem, reps, percent1rm, resttime, feedbackreps));
                    }
                }
                oldDays.Add(day);
            }
            newDays.Clear();

            foreach (WorkoutItem item in updatedItems) {
                db.query(String.Format("UPDATE workoutitems SET (difficulty, onerepmax) = ({0}, {1}) WHERE uuid = '{2}';", item.difficulty, item.oneRepMax, item.uuid));
                foreach (WorkoutSet s in item.sets) {
                    db.query(String.Format("UPDATE workoutsets SET feedbackreps = {0} WHERE uuid = '{1}';", s.repsCompleted, s.uuid));
                }
            }
            updatedItems.Clear();

            db.query(command.ToString());
        }
        public WorkoutItem FindItem(string uuid) {
            for (int i = 0; i < oldDays.Count; i++) {
                for (int u = 0; u < oldDays[i].workoutItems.Count; u++) {
                    if (oldDays[i].workoutItems[u].uuid == uuid) {
                        return oldDays[i].workoutItems[u];
                    }
                }
            }
            throw new Exception("couldn't find workout item");
        }
        public List<WorkoutSession> GetAllSessions() {
            return oldDays.Concat(newDays).ToList();
        }
        public List<WorkoutItem> GetItemsWithExercise(Exercise ex) {
            return GetAllSessions().SelectMany(day => day.workoutItems.Where(item => item.ex.exerciseName == ex.exerciseName)).ToList();
        }
        public void AddDay(WorkoutSession day) {
            newDays.Add(day);
        }
        public void UpdateItem(WorkoutItem item) {
            updatedItems.Add(item);
        }
       

        public void Store() {
            StoreAllCalibrations();
            StoreAllWorkouts();
        }
        public void Dispose() {
            Console.WriteLine("Dispose Happened!");
        }

        public UserDataAccess(Database db, User user) {
            this.user = user;
            this.db = db;

            LoadAllCalibrations();
            LoadWorkoutsAfter(DateTime.Now - new TimeSpan(21, 0, 0, 0, 0));
        }
    }
}
