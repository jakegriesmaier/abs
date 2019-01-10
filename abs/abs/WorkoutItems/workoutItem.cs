using System.Collections.Generic;
using System.Linq;
using monopage;

namespace abs {
    public struct workoutItem {
        public string uuid;
        public Exercise ex;
        public List<set> sets;
        public int difficulty;

        public mpObject toJSON(User user) {
            mpObject result = new mpObject();

            result.addProperty("uuid", new mpValue(uuid));
            result.addProperty("exercise", ex.toJSON(user));
            result.addProperty("sets", new mpArray(sets.Select(set => set.toJSON()).ToArray()));
            result.addProperty("difficulty", new mpValue(difficulty));

            return result;
        }
    }

}
