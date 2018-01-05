using monopage;
using System.Collections.Generic;
using System.Linq;

namespace abs {
    public class Exercise {

        public Exercise(string exerciseName) {
            setExerciseName(exerciseName);
            setMainBodyPart();
            setIsCompound();
            setAreaNumber();
            setEquipmentRequired();
            setEquipmentRequired2();
            setWeightRequired();
            setRequiresWeight();
            setIsDoubleExercise();
            this.youtube = "https://www.youtube.com/embed/qYV8zuz2qA0";
        }

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
        public string specifiedArea {
            get {
                if (mainBodyPart == "Chest") {
                    if (areaNumber == 1) return "Middle Chest";
                    else if (areaNumber == 2) return "Upper Chest";
                    else return "Lower Chest";
                }
                else if (mainBodyPart == "Legs") {
                    if (areaNumber == 1) return "Quads";
                    else if (areaNumber == 2) return "Hamstrings";
                    else return "Calves";
                }
                else if (mainBodyPart == "Back") {
                    if (areaNumber == 1) return "Upper Back";
                    else if (areaNumber == 2) return "Lats";
                    else return "Lower Back";
                }
                else if (mainBodyPart == "Shoulders") {
                    if (areaNumber == 1) return "Front Deltiods";
                    else if (areaNumber == 2) return "Lateral Deltoids";
                    else return "Rear Deltoids/Traps";
                }
                else if (mainBodyPart == "Arms") {
                    if (areaNumber == 1) return "Biceps";
                    else if (areaNumber == 2) return "Triceps";
                    else return "Forearms";
                }
                else {
                    if (areaNumber == 1) return "Upper Abs";
                    else if (areaNumber == 2) return "Lower Abs";
                    else return "Obliques";
                }
            }
        }
        public bool isCompound;
        public int areaNumber;
        public bool requiresWeight;
        public string equipmentRequired;
        public string equipmentRequired2;
        public string weightRequired;
        public bool isDoubleExercise;
        public string youtube;

        public static HashSet<Exercise> getAllExercises(Database db) {
            var exercises = db.query("select * from allknownexercises");
            List<binaryData> exercise = exercises["exercise"];
            List<binaryData> mainbodypart = exercises["mainbodypart"];
            List<binaryData> iscompound = exercises["iscompound"];
            List<binaryData> areanumber = exercises["areanumber"];
            List<binaryData> requiresweight = exercises["requiresweight"];
            List<binaryData> equipmentrequired = exercises["equipmentrequired"];
            List<binaryData> equipmentrequired2 = exercises["equipmentrequired2"];
            List<binaryData> weightrequired = exercises["weightrequired"];
            List<binaryData> isdoubleexercise = exercises["isdoubleexercise"];

            HashSet<Exercise> res = new HashSet<Exercise>();

            for(int i = 0; i < exercise.Count; i++) {
                res.Add(new Exercise(
                    exercise[i].asString(),
                    mainbodypart[i].asString(),
                    iscompound[i].asBool(),
                    areanumber[i].asInt(),
                    requiresweight[i].asBool(),
                    equipmentrequired[i].asString(),
                    equipmentrequired2[i].asString(),
                    weightrequired[i].asString(),
                    isdoubleexercise[i].asBool()
                ));
            }

            //TODO: load all exercises with a single query
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
            foreach(Exercise ex in exercises) {
                if(equipmentAvailable.Contains(ex.equipmentRequired)) {
                    res.Add(ex);
                }
            }

            return res;
        }
        
        public static Exercise getByName(this HashSet<Exercise> ex, string name) {
            foreach(Exercise e in ex) {
                if (e.getExerciseName().Equals(name)) {
                    return e;
                }
            } return null;
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

        public static HashSet<Exercise> onlyCompoundsInSubgroup (HashSet<Exercise> exercises, int subGroup) {
            return new HashSet<Exercise>(exercises.Where(e => (e.areaNumber == subGroup && e.isCompound)));
        }

        public static HashSet<Exercise> getUnusedExercises (HashSet<Exercise> allAvailableExercises, HashSet<Exercise> usedExercises) {
            return new HashSet<Exercise>(allAvailableExercises.Where(e => !(usedExercises.Select(ex => ex.getExerciseName()).Contains(e.getExerciseName()))));
        }

        public string getExerciseName() {
            return this.exerciseName;
        }

        public string getMainBodyPart() {
            return this.mainBodyPart;
        }

        public bool getIsCompound() {
            return this.isCompound;
        }

        public int getAreaNumber() {
            return this.areaNumber;
        }

        public bool getRequiresWeight() {
            return this.requiresWeight;
        }

        public string getEquipmentRequired() {
            return this.equipmentRequired;
        }

        public string getEquipmentRequired2() {
            return this.equipmentRequired2;
        }

        public string getWeightRequired() {
            return this.weightRequired;
        }

        public bool getIsDoubleExercise() {
            return this.isDoubleExercise;
        }

        public void setExerciseName(string exerciseName) {
            this.exerciseName = exerciseName;
        }

        public void setMainBodyPart() {
            this.mainBodyPart = db.query("SELECT iscompound FROM allknownexercises WHERE exercise = '" + exerciseName + "'").ToString();
        }

        public void setIsCompound() {
                string temp = db.query("SELECT iscompound FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
            if (temp == "1") { this.isCompound = true; }
            else { this.isCompound = false; }
        }

        public void setAreaNumber() {
            string temp = db.query("SELECT areanumber FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
            if (temp == "1") this.areaNumber = 1;
            else if (temp == "2") this.areaNumber = 2;
            else this.areaNumber = 3;
        }

        public void setRequiresWeight() {
            string temp = db.query("SELECT requiresweight FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
            if (temp == "1") { this.requiresWeight = true; } 
            else { this.requiresWeight = false; }
        }

        public void setEquipmentRequired() {
            this.equipmentRequired = db.query("SELECT equipmentrequired FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
        }/// <summary>
        /// </summary>

        public void setEquipmentRequired2() {
            this.equipmentRequired2 = db.query("SELECT equipmentrequired2 FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
        }

        public void setWeightRequired() {
            this.weightRequired = db.query("SELECT weightrequired FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
        }

        public void setIsDoubleExercise() {
            string temp = db.query("SELECT isdoubleexercise FROM allknownexercises WHERE exercise = '" + exerciseName + "' AND mainbodypart ='" + mainBodyPart + "'").ToString();
            if (temp == "1") { this.isDoubleExercise = true; } else { this.isDoubleExercise = false; }
        }

    }

}
