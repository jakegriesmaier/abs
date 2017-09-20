using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    public class Util {
        public static int percent1RM(int percent, int weight) {
            return (int)(Math.Ceiling((weight * (percent / 100.0)) / 5) * 5);
        }
    }
}
