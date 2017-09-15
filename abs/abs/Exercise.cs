using monopage;

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
        }


        private Database db;//TODO adjust this class so that it has the correct data fields and make the getters retrieve the information from the database
                            //TODO(make any getters that are user specific for createplan to take the userID as a parameter)
                            //TODO create tiles that use the data
                            //TODO make all the backend information for the createprogram tile and the other tiles
                            //TODO the exercise is created based on the exercises name which is retried from the database elsewhere when the plan is being created

        public string exerciseName;
        private string mainBodyPart;
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
        }

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
