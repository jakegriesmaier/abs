using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    public static class Constants {
        public const int NUM_BODYPARTS = 6; //the program splits the body into 6 different bodyparts
        public const int SET_DURATION = 3; //3 minutes is the estimated time to complete 1 set 

        /// <summary>
        /// Different ways of organizing the reps for an exercise's sets
        /// </summary>
        public static class SetStyles {
            public static class FiveSetOptions {
                public const int NUM_OPTIONS = 5;
                public static class FiveByFive {
                    public const int set1 = 5;
                    public const int set2 = 5;
                    public const int set3 = 5;
                    public const int set4 = 5;
                    public const int set5 = 5;
                }

                public static class FiveByTen {
                    public const int set1 = 10;
                    public const int set2 = 10;
                    public const int set3 = 10;
                    public const int set4 = 10;
                    public const int set5 = 10;
                }

                public static class FiveByFifteen {
                    public const int set1 = 15;
                    public const int set2 = 15;
                    public const int set3 = 15;
                    public const int set4 = 15;
                    public const int set5 = 15;
                }

                public static class FiveSetPyramid {
                    public const int set1 = 15;
                    public const int set2 = 10;
                    public const int set3 = 5;
                    public const int set4 = 10;
                    public const int set5 = 15;
                }

                public static class FiveSetLadder {
                    public const int set1 = 12;
                    public const int set2 = 10;
                    public const int set3 = 8;
                    public const int set4 = 6;
                    public const int set5 = 4;
                }
            }
        }
    }
}
