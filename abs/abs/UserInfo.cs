using System;
using System.Collections.Generic;
using System.Linq;
using monopage;

namespace abs {
    public struct Calibration {
        public int reps;
        public int weight;
        public DateTime? recorded;

        public bool Exists => reps != -1;
        public double Value => (1 + reps / 30.0) * weight;
    }

    public struct WorkoutItem {
        public string uuid;
        public Exercise ex;
        public List<WorkoutSet> sets;
        public int difficulty;
        public double oneRepMax;

        public mpObject toJSON(UserDataAccess user) {
            mpObject result = new mpObject();

            result.addProperty("uuid", new mpValue(uuid));
            result.addProperty("exercise", ex.toJSON(user));
            result.addProperty("sets", new mpArray(sets.Select(set => set.toJSON()).ToArray()));
            result.addProperty("difficulty", new mpValue(difficulty));
            result.addProperty("onerepmax", new mpValue(oneRepMax));

            return result;
        }

        public WorkoutItem(mpObject data) {
            uuid = ((mpValue)data.getChild("uuid")).data.asString();
            ex = Exercise.getByName(((mpValue)((mpObject)data.getChild("exercise")).getChild("name")).data.asString());
            difficulty = ((mpValue)data.getChild("difficulty")).data.asInt();
            sets = new List<WorkoutSet>();
            oneRepMax = ((mpValue)data.getChild("onerepmax")).data.asDouble();
            foreach (mpObject set in ((mpArray)data.getChild("sets"))) {
                sets.Add(new WorkoutSet(set));
            }
        }
    }

    public struct WorkoutSet {
        public string uuid;
        public int reps;
        public int percent1RM;
        public bool doneWithRest;
        public TimeSpan restTime;

        public int repsCompleted;

        public mpObject toJSON() {
            return new mpObject(
                new mpProperty("uuid", new mpValue(uuid)),
                new mpProperty("reps", new mpValue(reps)),
                new mpProperty("percent1RM", new mpValue(percent1RM)),
                new mpProperty("restTimeSeconds", new mpValue(restTime.TotalSeconds)),
                new mpProperty("doneWithRest", new mpValue(doneWithRest)),
                new mpProperty("repsCompleted", new mpValue(repsCompleted))
            );
        }
        public WorkoutSet(mpObject data) {
            uuid = ((mpValue)data.getChild("uuid")).data.asString();
            reps = ((mpValue)data.getChild("reps")).data.asInt();
            percent1RM = ((mpValue)data.getChild("percent1RM")).data.asInt();
            restTime = TimeSpan.FromSeconds(((mpValue)data.getChild("restTimeSeconds")).data.asInt());
            doneWithRest = ((mpValue)data.getChild("doneWithRest")).data.asBool();
            repsCompleted = ((mpValue)data.getChild("repsCompleted")).data.asInt();
        }
    }
    
    public struct WorkoutSession {
        public List<WorkoutItem> workoutItems;
        public string primaryGroup;
        public string secondaryGroup;
        public DateTime date;
        public string uuid;

        public mpObject toJSON(UserDataAccess user) {
            mpObject result = new mpObject();

            result.addProperty("uuid", new mpValue(uuid));
            result.addProperty("primaryGroup", new mpValue(primaryGroup));
            result.addProperty("secondaryGroup", new mpValue(secondaryGroup));
            result.addProperty("date", new mpValue(date.ToString("yyyy-MM-dd")));
            result.addProperty("items", new mpArray(workoutItems.Select(item => item.toJSON(user)).ToArray()));

            return result;
        }
        public WorkoutSession(mpObject data) {
            uuid = ((mpValue)data.getChild("uuid")).data.asString();
            primaryGroup = ((mpValue)data.getChild("primaryGroup")).data.asString();
            secondaryGroup = ((mpValue)data.getChild("secondaryGroup")).data.asString();
            date = Util.ParseDate(((mpValue)data.getChild("uuid")).data.asString());
            workoutItems = new List<WorkoutItem>();
            foreach (mpObject item in ((mpArray)data.getChild("items"))) {
                workoutItems.Add(new WorkoutItem(item));
            }
        }
    }

    public enum BodyPart {
        Chest = 1,
        Back = 2,
        Arms = 3,
        Abs = 4,
        Legs = 5,
        Shoulders = 6
    }
}
