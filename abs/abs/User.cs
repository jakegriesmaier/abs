using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monopage;

namespace abs {
    public class User {
        private readonly Database _db;
        public readonly string email; //email

        public readonly string salt; //salt
        public readonly string passwordEmailHash; //hash(password + email)
        public readonly string passwordEmailHashSaltHash; //hash(passwordEmailHash + salt)
        
        Dictionary<string, double> exericse1RMs;
        

        public User(Database db, string email, string passwordEmailHash, string salt) {
            _db = db;
            this.email = email;
            this.salt = salt;
            this.passwordEmailHash = passwordEmailHash;
            this.passwordEmailHashSaltHash = Util.hash(passwordEmailHash + salt);
        }

    }

    public class UserManager {
        private Database _db;
        private List<User> _users;
        public readonly int maxLoadedUsers = 50;

        private bool isSanitary(params string[] values) {
            foreach (string value in values)
                for (int i = 0; i < value.Length; i++)
                    if (!(char.IsLetterOrDigit(value[i]) || value[i] == '+' || value[i] == '/' || value[i] == '\\' || value[i] == '=' || value[i] == '-' || value[i] == '.' || value[i] == '_' || value[i] == '@'))
                        return false;
            return true;
        }
        private bool emailTaken(string email) {
            foreach (User user in _users) {
                if (email == user.email) return true;
            }
            return _db.query("SELECT * FROM public.users WHERE email = '" + email + "'").Rows == 1;
        }

        private User queryUser(string email, string passwordEmailHash) {
            QueryResult query = _db.query("SELECT * FROM public.users WHERE email = '" + email + "'");

            if (query.Rows == 1) {
                string queryEmail = query.GetField(0, 0).asString();
                string querySalt = query.GetField(1, 0).asString();
                string queryPasswordEmailHashSaltHash = query.GetField(2, 0).asString();

                string passwordEmailHashSaltHash = Util.hash(passwordEmailHash + querySalt);
                if (passwordEmailHashSaltHash == queryPasswordEmailHashSaltHash) {
                    return new User(_db, email, passwordEmailHash, querySalt);
                } else {
                    throw new Exception("User Credentials Incorrect (In Query)");
                }
            } else {
                throw new Exception("User Not Found (In Query)");
            }
        }
        private User checkCachedUser(string email, string passwordEmailHash) {
            for (int i = 0; i < _users.Count; i++) {
                string cachedEmail = _users[i].email;
                string cachedSalt = _users[i].salt;
                string cachedPasswordEmailHash = _users[i].passwordEmailHash;

                if (email == cachedEmail) {
                    if (passwordEmailHash == cachedPasswordEmailHash) {
                        return _users[i];
                    } else {
                        throw new Exception("User Credentials Incorrect (In Cache)");
                    }
                }
            }
            return null;
        }

        public void clearCache() {
            _users.Clear();
        }

        public void deleteUser(string email, string passwordEmailHash) {
            if (!isSanitary(email, passwordEmailHash)) throw new Exception("Only a-z, A-Z, 0-9, +, /, = allowed\nemail = " + email + "\npeh = " + passwordEmailHash);

            User foundUser = getUser(email, passwordEmailHash);

            if (foundUser != null) {
                _db.query("DELETE FROM public.users WHERE email = '" + email + "'");
                _users.Remove(foundUser);
            } else {
                throw new Exception("User Not Found To Delete");
            }
        }
        public User createUser(string email, string passwordEmailHash) {
            if (!isSanitary(email, passwordEmailHash)) throw new Exception("Only a-z, A-Z, 0-9, +, /, = allowed\nemail = " + email + "\npeh = " + passwordEmailHash);

            if (emailTaken(email)) throw new Exception("Email Already In Use");

            string salt = Util.randomHash();

            _db.query("INSERT INTO public.users VALUES ('" + email + "', '" + salt + "', '" + Util.hash(passwordEmailHash + salt) + "')");

            User newUser = new User(_db, email, passwordEmailHash, salt);

            _users.Insert(0, newUser);
            if (_users.Count > maxLoadedUsers) _users.RemoveAt(_users.Count - 1);

            return newUser;
        }
        public User getUser(string email, string passwordEmailHash) {
            if (!isSanitary(email, passwordEmailHash)) throw new Exception("Only a-z, A-Z, 0-9, +, /, = allowed\nemail = " + email + "\npeh = " + passwordEmailHash);

            User foundUser = checkCachedUser(email, passwordEmailHash);
            if (foundUser != null) return foundUser;
            User queriedUser = queryUser(email, passwordEmailHash);
            _users.Insert(0, queriedUser);
            if(_users.Count > maxLoadedUsers) _users.RemoveAt(_users.Count - 1);
            return queriedUser;
        }

        public UserManager(Database db) {
            _db = db;
            _users = new List<User>();
        }
    }
}
