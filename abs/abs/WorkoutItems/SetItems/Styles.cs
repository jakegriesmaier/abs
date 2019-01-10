using System.Collections.Generic;

namespace abs {
    /// <summary>
    /// Contains the different styles of exercise sets that can be performed (Contains percent1RMs and reps)
    /// </summary>
    public static class Styles {

        public static readonly SetDetail[] FiveByTen = { TenRepSet, TenRepSet, TenRepSet, TenRepSet, TenRepSet };
        public static readonly SetDetail[] FiveByFive = { FiveRepSet, FiveRepSet, FiveRepSet, FiveRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveByFifteen = { FifteenRepSet, FifteenRepSet, FifteenRepSet, FifteenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FivePyramid = { FifteenRepSet, TenRepSet, FiveRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FiveReversePyramid = { FiveRepSet, TenRepSet, FifteenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveLatter = { FiveRepSet, FiveRepSet, TenRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] FiveReverseLatter = { FifteenRepSet, FifteenRepSet, TenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] FiveDeload = { DeloadSet, DeloadSet, DeloadSet, DeloadSet, DeloadSet };

        public static readonly SetDetail[] ThreeByTen = { TenRepSet, TenRepSet, TenRepSet };
        public static readonly SetDetail[] ThreeByFive = { FiveRepSet, FiveRepSet, FiveRepSet };
        public static readonly SetDetail[] ThreeByFifteen = { FifteenRepSet, FifteenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreePyramid = { TenRepSet, FifteenRepSet, TenRepSet };
        public static readonly SetDetail[] ThreeReversePyramid = { FifteenRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreeLatter = { FiveRepSet, TenRepSet, FifteenRepSet };
        public static readonly SetDetail[] ThreeReverseLatter = { FifteenRepSet, TenRepSet, FiveRepSet };
        public static readonly SetDetail[] ThreeDeload = { DeloadSet, DeloadSet, DeloadSet };

        #region SetTypes
        public static readonly SetDetail TenRepSet = new SetDetail { reps = 10, Percent1RM = 71, restTime = 60 };
        public static readonly SetDetail FiveRepSet = new SetDetail { reps = 5, Percent1RM = 81, restTime = 90 };
        public static readonly SetDetail FifteenRepSet = new SetDetail { reps = 15, Percent1RM = 64, restTime = 45 };
        public static readonly SetDetail DeloadSet = new SetDetail { reps = 10, Percent1RM = 64, restTime = 60 };
        #endregion

        #region FiveSetters
        public static readonly List<SetDetails> FiveSetStyles = new List<SetDetails>{
            new SetDetails{ sets = FiveByTen, weightedLikelihood = 10},
            new SetDetails{ sets = FiveByFive, weightedLikelihood = 3},
            new SetDetails{ sets = FiveByFifteen, weightedLikelihood = 3},
            new SetDetails{ sets = FivePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = FiveReversePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = FiveLatter, weightedLikelihood = 3},
            new SetDetails{ sets = FiveReverseLatter, weightedLikelihood = 3},
            new SetDetails{ sets = FiveDeload, weightedLikelihood = 1}
        };
        
        #endregion

        #region ThreeSetters
        public static readonly List<SetDetails> ThreeSetStyles = new List<SetDetails>{
            new SetDetails{ sets = ThreeByTen, weightedLikelihood = 10},
            new SetDetails{ sets = ThreeByFive, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeByFifteen, weightedLikelihood = 3},
            new SetDetails{ sets = ThreePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeReversePyramid, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeLatter, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeReverseLatter, weightedLikelihood = 3},
            new SetDetails{ sets = ThreeDeload, weightedLikelihood = 1}
        };
        #endregion

    }

}
