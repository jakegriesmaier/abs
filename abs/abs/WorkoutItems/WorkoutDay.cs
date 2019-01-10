using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monopage;

namespace abs {

    public class WorkoutDay {
        public List<workoutItem> workoutItems;
        public string primaryGroup;
        public string secondaryGroup;
        public DateTime date;
        public string uuid;

        public mpObject toJSON(User user) {
            mpObject result = new mpObject();

            result.addProperty("uuid", new mpValue(uuid));
            result.addProperty("primaryGroup", new mpValue(primaryGroup));
            result.addProperty("secondaryGroup", new mpValue(secondaryGroup));
            result.addProperty("date", new mpValue(date.ToString("yyyy-MM-dd")));
            result.addProperty("items", new mpArray(workoutItems.Select(item => item.toJSON(user)).ToArray()));

            return result;
        }
    }

}
