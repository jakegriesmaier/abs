using monopage;
using System.Collections.Generic;
using System.Linq;

namespace abs {
    public class Exercise {
        private Exercise(string ExerciseName, string mainBodypart, bool isCompound, int areaNumber, bool requiresWeight, string equipmentRequired, string equipmentRequired2, string weightrequired, bool isDoubleExercise) {
            this.exerciseName = ExerciseName;
            this.mainBodyPart = mainBodypart;
            this.isCompound = isCompound;
            this.areaNumber = areaNumber;
            this.requiresWeight = requiresWeight;
            this.equipmentRequired = equipmentRequired;
            this.equipmentRequired = equipmentRequired2;
            this.weightRequired = weightrequired;
            this.isDoubleExercise = isDoubleExercise;
            this.youtube = "https://www.youtube.com/embed/qYV8zuz2qA0";
        }


        private Database db;//TODO adjust this class so that it has the correct data fields and make the getters retrieve the information from the database
                            //TODO(make any getters that are user specific for createplan to take the userID as a parameter)
                            //TODO create tiles that use the data
                            //TODO make all the backend information for the createprogram tile and the other tiles
                            //TODO the exercise is created based on the exercises name which is retried from the database elsewhere when the plan is being created

        public string exerciseName;
        public string mainBodyPart { get; private set; }
        public int areaNumber;
        public string specifiedArea {
            get {
                if (mainBodyPart == "Chest") {
                    if (areaNumber == 1) return "Middle Chest";
                    else if (areaNumber == 2) return "Upper Chest";
                    else return "Lower Chest";
                } else if (mainBodyPart == "Legs") {
                    if (areaNumber == 1) return "Quads";
                    else if (areaNumber == 2) return "Hamstrings";
                    else return "Calves";
                } else if (mainBodyPart == "Back") {
                    if (areaNumber == 1) return "Upper Back";
                    else if (areaNumber == 2) return "Lats";
                    else return "Lower Back";
                } else if (mainBodyPart == "Shoulders") {
                    if (areaNumber == 1) return "Front Deltiods";
                    else if (areaNumber == 2) return "Lateral Deltoids";
                    else return "Rear Deltoids/Traps";
                } else if (mainBodyPart == "Arms") {
                    if (areaNumber == 1) return "Biceps";
                    else if (areaNumber == 2) return "Triceps";
                    else return "Forearms";
                } else {
                    if (areaNumber == 1) return "Upper Abs";
                    else if (areaNumber == 2) return "Lower Abs";
                    else return "Obliques";
                }
            }
        }
        public bool isCompound;
        public bool requiresWeight;
        public string equipmentRequired;
        public string equipmentRequired2;
        public string weightRequired;
        public bool isDoubleExercise;
        public string youtube;

        public static HashSet<Exercise> getAllExercises(Database db) {
            var exercises = db.query("select * from exercises where CAST(requiresweight AS int) = 1");
            HashSet<Exercise> res = new HashSet<Exercise>();

            for (int i = 0; i < exercises.Rows; i++) {
                res.Add(new Exercise(
                    exercises.GetField("exercise", i).asString(),
                    exercises.GetField("mainbodypart", i).asString(),
                    exercises.GetField("iscompound", i).asBool(),
                    exercises.GetField("areanumber", i).asInt(),
                    exercises.GetField("requiresweight", i).asBool(),
                    exercises.GetField("equipmentrequired", i).asString(),
                    exercises.GetField("equipmentrequired2", i).asString(),
                    exercises.GetField("weightrequired", i).asString(),
                    exercises.GetField("isdoubleexercise", i).asBool()
                ));
            }

            //TODO: load all exercises with a single query
            globalExercises = res;
            return res;
        }

        public static HashSet<Exercise> availableWithEquipment(HashSet<Exercise> exercises, HashSet<string> equipmentAvailable, HashSet<string> equipmentAvailable2, HashSet<string> weightRequired) {
            HashSet<Exercise> res = new HashSet<Exercise>();
            foreach (Exercise ex in exercises) {
                if (equipmentAvailable.Contains(ex.equipmentRequired) && equipmentAvailable2.Contains(ex.equipmentRequired2) && weightRequired.Contains(ex.weightRequired)) {
                    res.Add(ex);
                }
            }
            return res;
        }

        public static HashSet<Exercise> availableWithEquipment(HashSet<Exercise> exercises, HashSet<string> equipmentAvailable) {
            HashSet<Exercise> res = new HashSet<Exercise>();
            foreach (Exercise ex in exercises) {
                if (equipmentAvailable.Contains(ex.equipmentRequired)) {
                    res.Add(ex);
                }
            }

            return res;
        }

        public static HashSet<Exercise> globalExercises;

        public static Exercise getByName(string name) {
            return getByName(globalExercises, name);
        }
        public static Exercise getByName(HashSet<Exercise> ex, string name) {
            foreach (Exercise e in ex) {
                if (e.exerciseName.Equals(name)) {
                    return e;
                }
            }
            return null;
        }

        public static HashSet<Exercise> whereAreaIs(HashSet<Exercise> exercises, string mainbodypart) {
            HashSet<Exercise> res = new HashSet<Exercise>();
            foreach (Exercise ex in exercises) {
                if (string.Compare(ex.mainBodyPart, mainbodypart, true) == 0) {
                    res.Add(ex);
                }
            }

            return res;
        }

        public static HashSet<Exercise> onlycompound(HashSet<Exercise> exercises) {
            return new HashSet<Exercise>(exercises.Where(e => e.isCompound));
        }

        public static HashSet<Exercise> subgroup(HashSet<Exercise> exercises, int subgroup) {
            return new HashSet<Exercise>(exercises.Where(e => (e.areaNumber == subgroup)));
        }

        public static HashSet<Exercise> onlyCompoundsInSubgroup(HashSet<Exercise> exercises, int subGroup) {
            return new HashSet<Exercise>(exercises.Where(e => (e.areaNumber == subGroup && e.isCompound)));
        }

        public static HashSet<Exercise> getUnusedExercises(HashSet<Exercise> allAvailableExercises, HashSet<Exercise> usedExercises) {
            return new HashSet<Exercise>(allAvailableExercises.Where(e => !(usedExercises.Select(ex => ex.exerciseName).Contains(e.exerciseName))));
        }

        public mpObject toJSON(UserInfo user) {
            return new mpObject(
               new mpProperty("name", new mpValue(exerciseName)), 
               new mpProperty("video", new mpValue(youtube)),
               new mpProperty("user1RM", new mpValue(user.GetOneRepMax(exerciseName).value)),
               new mpProperty("recommendedCalibrationWeight", new mpValue(25)),
               new mpProperty("calibrationWeight", new mpValue(25)),
               new mpProperty("calibrationReps", new mpValue(-1)),
               new mpProperty("hasBeenCalibrated", new mpValue(user.GetOneRepMax(exerciseName).exists)));
        }
    }

}
