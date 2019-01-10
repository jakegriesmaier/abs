using System;
using monopage;

namespace abs {
    public class set {
        public int reps;
        public int percent1RM;
        public bool doneWithRest = false;
        public TimeSpan restTime;
        
        public int repsCompleted;

        public mpObject toJSON() {
            return new mpObject(
                new mpProperty("reps", new mpValue(reps)),
                new mpProperty("percent1RM", new mpValue(percent1RM)),
                new mpProperty("restTimeSeconds", new mpValue(restTime.TotalSeconds)),
                new mpProperty("doneWithRest", new mpValue(doneWithRest)),
                new mpProperty("repsCompleted", new mpValue(repsCompleted))
            );
        }
    }

}
