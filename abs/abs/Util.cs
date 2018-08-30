using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public static Random r;
        public static int rand(int max) {
            if (r == null) r = new Random();
            return r.Next(max);
        }
        public static string randomHash() {
            if (r == null) r = new Random();
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
    }
}
