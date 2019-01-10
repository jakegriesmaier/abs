namespace abs {
    //experience level, gender, workout length, workout times per week, goal

    public struct muscleGroup {
        public string mainBodyPart;
        public int subGroup;

        public string subgroup {
            get {
                if (mainBodyPart == "Chest") {
                    if (subGroup == 1) return "Middle Chest";
                    else if (subGroup == 2) return "Upper Chest";
                    else return "Lower Chest";
                } else if (mainBodyPart == "Legs") {
                    if (subGroup == 1) return "Quads";
                    else if (subGroup == 2) return "Hamstrings";
                    else return "Calves";
                } else if (mainBodyPart == "Back") {
                    if (subGroup == 1) return "Upper Back";
                    else if (subGroup == 2) return "Lats";
                    else return "Lower Back";
                } else if (mainBodyPart == "Shoulders") {
                    if (subGroup == 1) return "Front Deltiods";
                    else if (subGroup == 2) return "Lateral Deltoids";
                    else return "Rear Deltoids/Traps";
                } else if (mainBodyPart == "Arms") {
                    if (subGroup == 1) return "Biceps";
                    else if (subGroup == 2) return "Triceps";
                    else return "Forearms";
                } else {
                    if (subGroup == 1) return "Upper Abs";
                    else if (subGroup == 2) return "Lower Abs";
                    else return "Obliques";
                }
            }
        }

        public string readable() {
            return mainBodyPart + " - " + subgroup;
        }
    }

}
