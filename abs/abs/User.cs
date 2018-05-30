using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monopage;

namespace abs {
    public class User {
        private Database db;
        public readonly string username;
        readonly Plan plan;

        public Plan getPlan() {
            throw new NotImplementedException();
        }

        public List<workoutItem> getExercises() {
            throw new NotImplementedException();
        }


        public User(Database db, string name, string password) {
            username = name;
            //db.query("SELECT")

        }

    }

    public class UserManager {
        private List<User> users;

        public User GetUser(string username, string password) {
            return null;
        }
    }
}
