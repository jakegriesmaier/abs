using System;
using System.Collections.Generic;
using System.Linq;
using monopage;


namespace abs {
    public class UserInfo {
        public struct OneRepMax {
            public int reps;
            public int weight;
            public DateTime? recorded;

            public bool exists => reps != -1;
            public double value => (1 + reps / 30.0) * weight;
        }

        public User user { get; private set; }
        private Database db;
        private Dictionary<string, List<OneRepMax>> user1rms;
        private Dictionary<string,List<OneRepMax>> new1rms;

        private void AddLoadedOneRepMax(string exercise, int reps, int weight, DateTime date) {
            OneRepMax oneRepMax = new OneRepMax { reps = reps, weight = weight, recorded = date };

            if (!user1rms.ContainsKey(exercise)) {
                user1rms.Add(exercise, new List<OneRepMax>());
            }
            user1rms[exercise].Add(oneRepMax);
        }
        public OneRepMax GetOneRepMax(string exercise) {
            if(user1rms.ContainsKey(exercise)) {
                List<OneRepMax> oneRMs = user1rms[exercise];
                //return oneRMs?.Last() ?? new OneRepMax { reps = -1, weight = -1, recorded = null };
                return (oneRMs != null) ? oneRMs.Last() : new OneRepMax { reps = -1, weight = -1, recorded = null };
            } else {
                return new OneRepMax { reps = -1, weight = -1, recorded = null };
            }
        }
        public void IngestCalibrationInfo(string exercise, int reps, int weight) {
            OneRepMax oneRepMax = new OneRepMax { reps = reps, weight = weight, recorded = DateTime.Now };

            if (!user1rms.ContainsKey(exercise)) {
                user1rms.Add(exercise, new List<OneRepMax>());
            }
            user1rms[exercise].Add(oneRepMax);

            if (!new1rms.ContainsKey(exercise)) {
                new1rms.Add(exercise, new List<OneRepMax>());
            }
            new1rms[exercise].Add(oneRepMax);
        }

        public void Store() {
            foreach(var exs in new1rms) {
                foreach(var ex in exs.Value) {
                    db.query($"INSERT INTO userinfo  VALUES ('{user.email}','{exs.Key}', {ex.reps}, {ex.weight},'{Util.DateStringFormat(ex.recorded.Value)}')");
                }
                exs.Value.Clear();
            }
        }
        public UserInfo(Database db, User user) {
            user1rms = new Dictionary<string, List<OneRepMax>>();
            new1rms = new Dictionary<string, List<OneRepMax>>();

            this.user = user;
            this.db = db;

            var data = db.query($"SELECT * FROM userinfo WHERE email = '{user.email}'");
            for(int i = 0; i < data.Rows; i++) {
                var row = data.GetRow(i);
                AddLoadedOneRepMax(row[1].asString(), row[2].asInt(), row[3].asInt(), row[4].asDate());
            }
        }
    }
}
