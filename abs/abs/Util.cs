using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    public static class Util {
        public static int calculate1RM(int percent, int weight) {
            return (int)(Math.Ceiling((weight * (percent / 100.0)) / 5) * 5);
        }

        /// <summary>
        /// calculates the max percentage of a one rep max can be lifted if performing this number of reps
        /// </summary>
        /// <param name="reps">the number of reps we are checking the highest possible weight for</param>
        /// <returns>the maximum possible percent of one rep max as an int</returns>
        public static int repsMaxPercent1RM(int reps) {
            if(reps == 1) {
                return 100;
            } else {
                int basis = 95;
                return (int)Math.Max(0, Math.Ceiling(basis - ((reps - 2) * 2.5)));
            }
        }

        public static T randomElement<T>(this HashSet<T> set) {
            if (set.Count == 0) throw new Exception("randomElement can't take an empty HashSet<...>");
            return set.ToArray()[rand(set.Count)];
        }
        public static HashSet<T> selectRandomUniqueN<T>(this HashSet<T> set, int N) {
            HashSet<T> copy = new HashSet<T>(set);
            HashSet<T> res = new HashSet<T>();
            for(int i = 0; i < N; ++i) {
                T selected = copy.randomElement();
                res.Add(selected);
                copy.Remove(selected);
            }
            return res;
        }

        public static SHA256Managed hasher = null;
        public static string hash(string val) {
            //if(hasher == null) hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            if (hasher == null) hasher = new SHA256Managed();
            
            byte[] rawBytes = Encoding.UTF8.GetBytes(val);
            byte[] hashedBytes = hasher.ComputeHash(rawBytes);
            byte[] base64Bytes = hashedBytes;
            String plainResult = Convert.ToBase64String(base64Bytes);


            return plainResult;
        }

        public const int seed = 74975;
        public static Random r;
        public static int rand(int max) {
            if (r == null) r = new Random(seed);
            return r.Next(max);
        }
        public static double rand(double start, double end) {
            if (r == null) r = new Random(seed);
            return start + r.NextDouble() * (end - start);
        }
        public static string randomHash() {
            if (r == null) r = new Random(seed);
            return hash(r.Next().ToString());
        }

        public static string DateStringFormat(DateTime date) {
            return date.Year + date.Month.ToString().PadLeft(2, '0') + date.Day.ToString().PadLeft(2, '0');
        }
        public static DateTime ParseDate(String date) {
            int year = int.Parse(date.Substring(0, 4));
            int month = int.Parse(date.Substring(5, 7)) - 1;
            int day = int.Parse(date.Substring(8, 10));

            return new DateTime(year, month, day);
        }

        public static bool IsEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
        public static bool IsBase64(string value) {
            if (value == null || value.Length == 0 ||
                value.Contains(" ") || value.Contains("\t") || value.Contains("\r") ||
                value.Contains("\n"))
                return false;
            try {
                Convert.FromBase64String(value);
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// returns a list of a static classes fields values
        /// </summary>
        public static List<T> GetStaticClassesFieldValues<T>(Type t) {
            FieldInfo[] fields = t.GetFields();
            List<T> values = new List<T>();
            for (int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                object val = field.GetRawConstantValue();
                if (val.GetType().Equals(typeof(T))) {
                    values.Add((T)field.GetRawConstantValue());
                }
            }
            return values;
        }
        
    }
}
