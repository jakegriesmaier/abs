namespace abs {
    public class Exercise {

        public string exerciseName {
            get; private set;
        }
        private BodyPart mainBodyPart;
        public string specifiedArea {
            get {
                if (mainBodyPart == BodyPart.Chest) {
                    if (areaNumber == 1) return "Middle Chest";
                    else if (areaNumber == 2) return "Upper Chest";
                    else return "Lower Chest";
                }
                else if (mainBodyPart == BodyPart.Legs) {
                    if (areaNumber == 1) return "Quads";
                    else if (areaNumber == 2) return "Hamstrings";
                    else return "Calves";
                }
                else if (mainBodyPart == BodyPart.Back) {
                    if (areaNumber == 1) return "Upper Back";
                    else if (areaNumber == 2) return "Lats";
                    else return "Lower Back";
                }
                else if (mainBodyPart == BodyPart.Shoulders) {
                    if (areaNumber == 1) return "Front Deltiods";
                    else if (areaNumber == 2) return "Lateral Deltoids";
                    else return "Rear Deltoids/Traps";
                }
                else if (mainBodyPart == BodyPart.Arms) {
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
        private LiftType liftType;
        private int areaNumber;
        private bool requiresWeight;
        public int[] reps {
            get; private set;
        }
        public int sets {
            get; private set;
        }
        private string style;
        public int[] weights {
            get; private set;
        }
        public string equipment {
            get; private set;
        }

        public Exercise() {

        }

        public enum BodyPart {
            Chest = 1, 
            Legs = 2,
            Back = 3,
            Shoulders = 4, 
            Arms = 5,
            Abs = 6
        }

        public enum LiftType {
            Compound = 1,
            Isolation = 2
        }

    }

}
