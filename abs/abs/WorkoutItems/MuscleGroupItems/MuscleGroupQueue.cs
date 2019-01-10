using System;

namespace abs {
    public class MuscleGroupQueue {
        public readonly string mainBodyPart;

        public readonly double scalingFactor;
        public double timePutIn => totalExercises / scalingFactor;
        public int totalExercises {
            get {
                return groups.Aggregate((accumulator, initial) => accumulator + initial);
            }
        }
        public int[] groups = new int[3] { 0, 0, 0 };

        private void addToSubgroup(int group) {
            groups[group - 1]++;
        }
        private void removeFromGroup(int group) {
            groups[group - 1]--;
        }

        public void reset() {
            groups[0] = 0;
            groups[1] = 0;
            groups[2] = 0;
        }

        public muscleGroup generateGroupExercise() {
            int subgroup = Array.IndexOf(groups, groups.Min()) + 1;
            addToSubgroup(subgroup);
            return new muscleGroup { mainBodyPart = mainBodyPart, subGroup = subgroup };
        }
        public void undoGroupExercise(muscleGroup group) {
            removeFromGroup(group.subGroup);
        }

        public MuscleGroupQueue(string mainBodyPart, double factor) {
            scalingFactor = factor;
            this.mainBodyPart = mainBodyPart;
        }
    }

}
