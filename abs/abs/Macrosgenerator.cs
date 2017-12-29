using monopage;

namespace abs {

    public struct MacrosDefinition {
        public double weight;//users current weight
        public string goal;//users goal (gain muscle, lose fat, maintain weight)
    }


    public class MacrosGenerator {

        public MacrosDefinition usersInput;
        public double protein;//the number of grams of protein to be consumed
        public double fat;//the number of grams of fat to be consumed
        public double carbohydrates;//the number of grams of carbohydrates to be consumed
        public double calories;//the total number of calories to be consumed
        public int calorieMultiplier;
        public double[] macroMultiplier;//protein-fat grams multipliers

        public MacrosGenerator(MacrosDefinition input) {
            this.usersInput = input;
            this.protein = 0;
            this.fat = 0;
            this.carbohydrates = 0;
            this.calorieMultiplier = 0;
            this.macroMultiplier = new double[2];

            
            calculateCalorieMultiplier();
            calculateCalories();
            calculateMacroRatio();
            calculateMacros();
        }


        public void calculateCalorieMultiplier() {
            if (usersInput.goal == "Gain") {
                calorieMultiplier = 20;
            } else if (usersInput.goal == "Lose") {
                calorieMultiplier = 13;
            } else {
                calorieMultiplier = 16;
            }
        }

        //calculates the calories the user is to consume on a  daily basis
        public void calculateCalories() {
            calories = usersInput.weight * calorieMultiplier;
        }

        public void calculateMacroRatio() {
            if (usersInput.goal == "Gain") {
                macroMultiplier = new double[2] { .8, .3 };
            }else if (usersInput.goal == "Lose") {
                macroMultiplier = new double[2] { 2, .3 };
            } else {
                macroMultiplier = new double[2] { 1, .3 };
            }
        }

        public void calculateMacros() {
            protein = macroMultiplier[0] * usersInput.weight;
            fat = macroMultiplier[1] * usersInput.weight;
            carbohydrates = calories - ((protein * 4) + (fat * 9)) / 4;
        }


    }


}
