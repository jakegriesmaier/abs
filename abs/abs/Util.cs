using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    public static class Util {
        public static int percent1RM(int percent, int weight) {
            return (int)(Math.Ceiling((weight * (percent / 100.0)) / 5) * 5);
        }

        public static T randomElement<T>(this HashSet<T> set) {
            if (set.Count == 0) throw new Exception("randomElement can't take an empty HashSet<...>");
            return set.ToArray()[rand(set.Count)];
        }

        public static Random r;
        public static int rand(int max) {
            if (r == null) r = new Random(54581);
            return r.Next(max);
        }
    }
}
